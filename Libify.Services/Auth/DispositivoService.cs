using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Libify.Services.Auth
{
    /// <summary>
    /// Gestão das sessões/dispositivos do usuário autenticado (tenant atual).
    /// Revogar um dispositivo também revoga seus refresh tokens.
    /// </summary>
    public class DispositivoService : IDispositivoService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;
        private readonly HybridCache _cache;

        public DispositivoService(AppDbContext db, ITenantContext tenant, HybridCache cache)
        {
            _db = db;
            _tenant = tenant;
            _cache = cache;
        }

        public async Task<IEnumerable<Dispositivo>> ListarAsync(CancellationToken cancellationToken = default)
            => await _db.Dispositivo
                .Where(d => !d.Revogado)
                .OrderByDescending(d => d.UltimoAcessoEm)
                .ToListAsync(cancellationToken);

        public async Task<bool> RevogarAsync(Guid dispositivoId, CancellationToken cancellationToken = default)
        {
            var dispositivo = await _db.Dispositivo.FirstOrDefaultAsync(d => d.Id == dispositivoId, cancellationToken);
            if (dispositivo is null)
                return false;

            await RevogarInternoAsync(dispositivo, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> RevogarOutrosAsync(CancellationToken cancellationToken = default)
        {
            var atual = _tenant.DispositivoId;
            var dispositivos = await _db.Dispositivo
                .Where(d => !d.Revogado && d.Id != atual)
                .ToListAsync(cancellationToken);

            foreach (var dispositivo in dispositivos)
                await RevogarInternoAsync(dispositivo, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return dispositivos.Count;
        }

        private async Task RevogarInternoAsync(Dispositivo dispositivo, CancellationToken cancellationToken)
        {
            var agora = DateTime.UtcNow;
            dispositivo.Revogado = true;
            dispositivo.RevogadoEm = agora;
            dispositivo.UpdatedAt = agora;
            await _cache.RemoveAsync(DispositivoRevocationCache.Key(dispositivo.Id), cancellationToken);

            var tokens = await _db.RefreshToken
                .Where(r => r.DispositivoId == dispositivo.Id && !r.Revogado)
                .ToListAsync(cancellationToken);
            foreach (var token in tokens)
            {
                token.Revogado = true;
                token.RevogadoEm = agora;
                token.UpdatedAt = agora;
            }
        }
    }
}
