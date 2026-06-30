using Libify.Domain.Ports;

namespace Libify.Test.Helpers
{
    public class NullMessageBus : IMessageBus
    {
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            => Task.CompletedTask;
    }
}
