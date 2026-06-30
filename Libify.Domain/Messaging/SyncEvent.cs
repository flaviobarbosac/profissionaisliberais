namespace Libify.Domain.Messaging
{
    public enum SyncOperacao
    {
        Upsert = 1,
        Delete = 2
    }

    /// <summary>
    /// Evento de sincronização de uma entidade. Por ser genérico fechado por tipo
    /// (SyncEvent&lt;Cliente&gt;, SyncEvent&lt;Servico&gt;...), o MassTransit cria uma
    /// fila/endpoint por módulo, evitando gargalo entre módulos.
    /// </summary>
    public record SyncEvent<T> where T : class
    {
        public Guid EntityId { get; init; }
        public Guid UsuarioId { get; init; }
        public long Version { get; init; }
        public SyncOperacao Operacao { get; init; }
        public T? Payload { get; init; }
        public DateTime OcorridoEm { get; init; }
    }
}
