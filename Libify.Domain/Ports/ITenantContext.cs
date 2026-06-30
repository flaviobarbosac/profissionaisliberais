namespace Libify.Domain.Ports
{
    /// <summary>
    /// Contexto do tenant atual (resolvido do JWT pelo middleware). Scoped por requisição.
    /// </summary>
    public interface ITenantContext
    {
        Guid? UsuarioId { get; }
        Guid? DispositivoId { get; }
        bool IsAuthenticated { get; }
        void Set(Guid usuarioId, Guid? dispositivoId);
    }
}
