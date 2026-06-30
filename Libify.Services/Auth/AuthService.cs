using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Observability;
using Libify.Infraestructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libify.Services.Auth
{
    /// <summary>
    /// Autenticação via login social Google (id_token) + JWT próprio com refresh por dispositivo.
    /// Consultas pré-autenticação usam IgnoreQueryFilters (tenant ainda não resolvido).
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _token;
        private readonly IGoogleAuthClient _google;
        private readonly JwtOptions _jwt;
        private readonly ILogger<AuthService> _logger;
        private readonly LibifyMetrics? _metrics;

        public AuthService(
            AppDbContext db,
            ITokenService token,
            IGoogleAuthClient google,
            IOptions<JwtOptions> jwt,
            ILogger<AuthService> logger,
            LibifyMetrics? metrics = null)
        {
            _db = db;
            _token = token;
            _google = google;
            _jwt = jwt.Value;
            _logger = logger;
            _metrics = metrics;
        }

        public async Task<AuthResult> LoginGoogleAsync(string idToken, DispositivoInfo info, CancellationToken cancellationToken = default)
        {
            GoogleUserInfo perfil;
            try
            {
                perfil = await _google.ValidarIdTokenAsync(idToken, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                _metrics?.LoginFalha("token_invalido");
                throw;
            }

            if (!perfil.EmailVerificado || string.IsNullOrWhiteSpace(perfil.Email))
            {
                _metrics?.LoginFalha("email_nao_verificado");
                throw new InvalidOperationException("E-mail do Google não verificado.");
            }

            var agora = DateTime.UtcNow;
            var email = perfil.Email.Trim().ToLowerInvariant();

            var usuario = await _db.Usuario.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.DeletedAt == null && (u.GoogleId == perfil.Subject || u.Email == email), cancellationToken);

            if (usuario is null)
            {
                usuario = new Usuario
                {
                    Id = Guid.CreateVersion7(),
                    Nome = string.IsNullOrWhiteSpace(perfil.Nome) ? email : perfil.Nome!,
                    Email = email,
                    GoogleId = perfil.Subject,
                    FotoUrl = perfil.FotoUrl,
                    CreatedAt = agora,
                    UpdatedAt = agora,
                    Version = 1
                };
                _db.Usuario.Add(usuario);
                _logger.LogInformation("Novo usuário criado via login Google: {Email}", email);
            }
            else
            {
                usuario.GoogleId ??= perfil.Subject;
                if (string.IsNullOrWhiteSpace(usuario.Email))
                    usuario.Email = email;
                usuario.FotoUrl = perfil.FotoUrl ?? usuario.FotoUrl;
                usuario.UpdatedAt = agora;
            }

            var dispositivo = await RegistrarDispositivoAsync(usuario.Id, info, agora, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            _metrics?.LoginGoogle();

            return await EmitirAsync(usuario, dispositivo, agora, cancellationToken);
        }

        public async Task<AuthResult> RefreshAsync(string refreshToken, DispositivoInfo info, CancellationToken cancellationToken = default)
        {
            var agora = DateTime.UtcNow;
            var hash = _token.Hash(refreshToken);

            var rt = await _db.RefreshToken.IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.TokenHash == hash && !r.Revogado && r.DeletedAt == null && r.ExpiraEm > agora, cancellationToken);
            if (rt is null)
                throw new InvalidOperationException("Refresh token inválido.");

            var dispositivo = await _db.Dispositivo.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == rt.DispositivoId, cancellationToken);
            if (dispositivo is null || dispositivo.Revogado)
                throw new InvalidOperationException("Dispositivo revogado.");

            var usuario = await _db.Usuario.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == rt.UsuarioId, cancellationToken);
            if (usuario is null)
                throw new InvalidOperationException("Usuário inválido.");

            var novoPlain = _token.GerarRefreshToken();
            rt.Revogado = true;
            rt.RevogadoEm = agora;
            rt.SubstituidoPorHash = _token.Hash(novoPlain);
            rt.UpdatedAt = agora;

            dispositivo.UltimoAcessoEm = agora;
            dispositivo.Ip = info.Ip ?? dispositivo.Ip;
            dispositivo.UpdatedAt = agora;

            var (access, exp) = _token.GerarAccessToken(usuario, dispositivo.Id);
            var novo = new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UsuarioId = usuario.Id,
                DispositivoId = dispositivo.Id,
                TokenHash = _token.Hash(novoPlain),
                ExpiraEm = agora.AddDays(_jwt.RefreshTokenDays),
                CreatedAt = agora,
                UpdatedAt = agora,
                Version = 1
            };
            _db.RefreshToken.Add(novo);
            await _db.SaveChangesAsync(cancellationToken);

            return new AuthResult(usuario.Id, usuario.Nome, dispositivo.Id, access, exp, novoPlain, novo.ExpiraEm);
        }

        private async Task<Dispositivo> RegistrarDispositivoAsync(Guid usuarioId, DispositivoInfo info, DateTime agora, CancellationToken cancellationToken)
        {
            var dispositivo = await _db.Dispositivo.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId && d.DeviceId == info.DeviceId, cancellationToken);

            if (dispositivo is null)
            {
                dispositivo = new Dispositivo
                {
                    Id = Guid.CreateVersion7(),
                    UsuarioId = usuarioId,
                    DeviceId = info.DeviceId,
                    CreatedAt = agora,
                    UpdatedAt = agora,
                    Version = 1
                };
                _db.Dispositivo.Add(dispositivo);
            }

            dispositivo.Nome = info.Nome ?? dispositivo.Nome;
            dispositivo.Plataforma = info.Plataforma ?? dispositivo.Plataforma;
            dispositivo.AppVersion = info.AppVersion ?? dispositivo.AppVersion;
            dispositivo.Ip = info.Ip;
            dispositivo.UserAgent = info.UserAgent;
            dispositivo.UltimoAcessoEm = agora;
            dispositivo.Revogado = false;
            dispositivo.RevogadoEm = null;
            dispositivo.UpdatedAt = agora;
            return dispositivo;
        }

        private async Task<AuthResult> EmitirAsync(Usuario usuario, Dispositivo dispositivo, DateTime agora, CancellationToken cancellationToken)
        {
            var (access, exp) = _token.GerarAccessToken(usuario, dispositivo.Id);
            var refreshPlain = _token.GerarRefreshToken();

            var rt = new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UsuarioId = usuario.Id,
                DispositivoId = dispositivo.Id,
                TokenHash = _token.Hash(refreshPlain),
                ExpiraEm = agora.AddDays(_jwt.RefreshTokenDays),
                CreatedAt = agora,
                UpdatedAt = agora,
                Version = 1
            };
            _db.RefreshToken.Add(rt);
            await _db.SaveChangesAsync(cancellationToken);

            return new AuthResult(usuario.Id, usuario.Nome, dispositivo.Id, access, exp, refreshPlain, rt.ExpiraEm);
        }
    }
}
