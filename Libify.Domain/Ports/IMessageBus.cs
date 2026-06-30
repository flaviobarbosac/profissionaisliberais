namespace Libify.Domain.Ports
{
    /// <summary>
    /// Port de mensageria. Adapter dev = RabbitMQ (MassTransit); prod = Amazon SQS.
    /// </summary>
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
