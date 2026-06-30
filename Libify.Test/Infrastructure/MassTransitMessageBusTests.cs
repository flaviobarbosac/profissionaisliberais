using FluentAssertions;
using Libify.Infraestructure.Messaging;
using MassTransit;
using Moq;

namespace Libify.Test.Infrastructure
{
    public class MassTransitMessageBusTests
    {
        public record MensagemTeste(string Valor);

        [Fact]
        public async Task PublishAsync_DelegaParaPublishEndpoint()
        {
            var endpoint = new Mock<IPublishEndpoint>();
            endpoint
                .Setup(e => e.Publish(It.IsAny<MensagemTeste>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var bus = new MassTransitMessageBus(endpoint.Object);
            var msg = new MensagemTeste("ok");

            await bus.PublishAsync(msg);

            endpoint.Verify(e => e.Publish(msg, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
