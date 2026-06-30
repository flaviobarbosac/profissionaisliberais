using FluentAssertions;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Security;
using Libify.Services.Auth;
using Libify.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Libify.Test.Services
{
    public class AuthServiceTests
    {
        private static AuthService Criar(out FakeGoogleAuthClient google, AppDbContext context)
        {
            var jwt = Options.Create(new JwtOptions
            {
                Key = new string('k', 40),
                Issuer = "Libify",
                Audience = "Libify",
                AccessTokenMinutes = 60,
                RefreshTokenDays = 30
            });
            var token = new TokenService(jwt);
            google = new FakeGoogleAuthClient();
            return new AuthService(context, token, google, jwt, NullLogger<AuthService>.Instance);
        }

        [Fact]
        public async Task LoginGoogle_EmiteAccessERefresh()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out _, context);

            var result = await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));

            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.UsuarioId.Should().NotBe(Guid.Empty);
            result.DispositivoId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task LoginGoogle_CriaUsuarioComEmailEGoogleId()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out var google, context);
            google.Proximo = new GoogleUserInfo("sub-123", "Pessoa@Gmail.com", true, "Pessoa", "http://foto");

            var result = await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));

            var usuario = context.Usuario.IgnoreQueryFilters().Single(u => u.Id == result.UsuarioId);
            usuario.Email.Should().Be("pessoa@gmail.com");
            usuario.GoogleId.Should().Be("sub-123");
        }

        [Fact]
        public async Task LoginGoogle_UsuarioExistente_Reutiliza()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out _, context);

            var primeiro = await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));
            var segundo = await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));

            segundo.UsuarioId.Should().Be(primeiro.UsuarioId);
            segundo.DispositivoId.Should().Be(primeiro.DispositivoId);
        }

        [Fact]
        public async Task LoginGoogle_EmailNaoVerificado_LancaExcecao()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out var google, context);
            google.Proximo = new GoogleUserInfo("sub-1", "x@gmail.com", false, "X", null);

            var act = async () => await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Refresh_RotacionaToken()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out _, context);

            var login = await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));
            var refreshed = await auth.RefreshAsync(login.RefreshToken, new DispositivoInfo("device-1"));

            refreshed.AccessToken.Should().NotBeNullOrEmpty();
            refreshed.RefreshToken.Should().NotBe(login.RefreshToken);
        }

        [Fact]
        public async Task Refresh_TokenInvalido_LancaExcecao()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out _, context);

            var act = async () => await auth.RefreshAsync("nao-existe", new DispositivoInfo("device-1"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Refresh_DispositivoRevogado_LancaExcecao()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var auth = Criar(out _, context);

            var login = await auth.LoginGoogleAsync("id-token", new DispositivoInfo("device-1"));

            var dispositivo = context.Dispositivo.IgnoreQueryFilters().Single(d => d.Id == login.DispositivoId);
            dispositivo.Revogado = true;
            await context.SaveChangesAsync();

            var act = async () => await auth.RefreshAsync(login.RefreshToken, new DispositivoInfo("device-1"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
