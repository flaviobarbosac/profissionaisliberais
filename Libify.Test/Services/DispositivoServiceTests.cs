using FluentAssertions;
using Libify.Domain.Model;
using Libify.Infraestructure.Database;
using Libify.Services.Auth;
using Libify.Test.Helpers;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Libify.Test.Services
{
    public class DispositivoServiceTests
    {
        private static HybridCache NovoCache()
            => new ServiceCollection().AddHybridCache().Services.BuildServiceProvider().GetRequiredService<HybridCache>();

        private static Dispositivo NovoDispositivo(Guid usuarioId, string deviceId, bool revogado = false)
            => new()
            {
                Id = Guid.CreateVersion7(),
                UsuarioId = usuarioId,
                DeviceId = deviceId,
                Revogado = revogado,
                UltimoAcessoEm = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 1
            };

        [Fact]
        public async Task ListarAsync_RetornaSomenteNaoRevogados()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var ctx = TestDbContextHelper.CreateInMemory(tenant);
            ctx.Dispositivo.AddRange(
                NovoDispositivo(tenant.UsuarioId!.Value, "d1"),
                NovoDispositivo(tenant.UsuarioId!.Value, "d2", revogado: true));
            await ctx.SaveChangesAsync();

            var service = new DispositivoService(ctx, tenant, NovoCache());
            var lista = (await service.ListarAsync()).ToList();

            lista.Should().ContainSingle();
            lista[0].DeviceId.Should().Be("d1");
        }

        [Fact]
        public async Task RevogarAsync_RevogaDispositivoETokens()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var ctx = TestDbContextHelper.CreateInMemory(tenant);
            var disp = NovoDispositivo(tenant.UsuarioId!.Value, "d1");
            ctx.Dispositivo.Add(disp);
            ctx.RefreshToken.Add(new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UsuarioId = tenant.UsuarioId!.Value,
                DispositivoId = disp.Id,
                TokenHash = "hash",
                ExpiraEm = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 1
            });
            await ctx.SaveChangesAsync();

            var service = new DispositivoService(ctx, tenant, NovoCache());
            var ok = await service.RevogarAsync(disp.Id);

            ok.Should().BeTrue();
            disp.Revogado.Should().BeTrue();
            (await ctx.RefreshToken.FindAsync(ctx.RefreshToken.First().Id))!.Revogado.Should().BeTrue();
        }

        [Fact]
        public async Task RevogarAsync_Inexistente_RetornaFalse()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var ctx = TestDbContextHelper.CreateInMemory(tenant);
            var service = new DispositivoService(ctx, tenant, NovoCache());

            (await service.RevogarAsync(Guid.CreateVersion7())).Should().BeFalse();
        }

        [Fact]
        public async Task RevogarOutrosAsync_MantemDispositivoAtual()
        {
            var usuarioId = Guid.CreateVersion7();
            var atual = NovoDispositivo(usuarioId, "atual");
            var tenant = new TestTenantContext(usuarioId, atual.Id);
            using var ctx = TestDbContextHelper.CreateInMemory(tenant);
            ctx.Dispositivo.AddRange(
                atual,
                NovoDispositivo(usuarioId, "outro1"),
                NovoDispositivo(usuarioId, "outro2"));
            await ctx.SaveChangesAsync();

            var service = new DispositivoService(ctx, tenant, NovoCache());
            var revogados = await service.RevogarOutrosAsync();

            revogados.Should().Be(2);
            atual.Revogado.Should().BeFalse();
        }
    }
}
