using Libify.Domain.Ports;

namespace Libify.Test.Helpers
{
    /// <summary>
    /// Captura os eventos publicados para asserções de outbox/sync nos testes.
    /// </summary>
    public class CapturingMessageBus : IMessageBus
    {
        public List<object> Publicados { get; } = new();

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            Publicados.Add(message);
            return Task.CompletedTask;
        }
    }
}
