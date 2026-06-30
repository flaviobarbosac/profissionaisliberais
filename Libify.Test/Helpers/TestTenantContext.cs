using Libify.Domain.Ports;

namespace Libify.Test.Helpers
{
    public class TestTenantContext : ITenantContext
    {
        public TestTenantContext(Guid? usuarioId = null, Guid? dispositivoId = null)
        {
            UsuarioId = usuarioId;
            DispositivoId = dispositivoId;
        }

        public Guid? UsuarioId { get; private set; }
        public Guid? DispositivoId { get; private set; }
        public bool IsAuthenticated => UsuarioId.HasValue;

        public void Set(Guid usuarioId, Guid? dispositivoId)
        {
            UsuarioId = usuarioId;
            DispositivoId = dispositivoId;
        }
    }
}
