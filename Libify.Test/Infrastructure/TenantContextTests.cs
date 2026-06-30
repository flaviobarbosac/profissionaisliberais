using FluentAssertions;
using Libify.Infraestructure.Security;

namespace Libify.Test.Infrastructure
{
    public class TenantContextTests
    {
        [Fact]
        public void NovoContexto_NaoEstaAutenticado()
        {
            var ctx = new TenantContext();

            ctx.IsAuthenticated.Should().BeFalse();
            ctx.UsuarioId.Should().BeNull();
            ctx.DispositivoId.Should().BeNull();
        }

        [Fact]
        public void Set_DefineTenantEAutentica()
        {
            var ctx = new TenantContext();
            var usuario = Guid.CreateVersion7();
            var dispositivo = Guid.CreateVersion7();

            ctx.Set(usuario, dispositivo);

            ctx.IsAuthenticated.Should().BeTrue();
            ctx.UsuarioId.Should().Be(usuario);
            ctx.DispositivoId.Should().Be(dispositivo);
        }
    }
}
