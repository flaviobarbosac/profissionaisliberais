using Libify.Domain.Ports;
using MassTransit;

namespace Libify.Infraestructure.Messaging
{
    /// <summary>
    /// Adapter do port IMessageBus sobre o MassTransit. Quando há outbox transacional
    /// ativo, a publicação é capturada e entregue junto ao SaveChanges do DbContext.
    /// </summary>
    public class MassTransitMessageBus : IMessageBus
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitMessageBus(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            => _publishEndpoint.Publish(message, cancellationToken);
    }
}
