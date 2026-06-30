using Libify.Domain.Ports;

namespace Libify.Infraestructure.Security
{
    /// <summary>
    /// Holder scoped do tenant atual. Preenchido pelo middleware a partir das claims do JWT.
    /// </summary>
    public class TenantContext : ITenantContext
    {
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
