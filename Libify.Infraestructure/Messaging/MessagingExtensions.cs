using System.Diagnostics.CodeAnalysis;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Libify.Infraestructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public static class MessagingExtensions
    {
        /// <summary>
        /// Configura MassTransit sobre RabbitMQ com Outbox transacional (EF/Postgres).
        /// Uma fila/endpoint é criada por consumidor (módulo), evitando gargalos.
        /// </summary>
        public static IServiceCollection AddLibifyMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator>? configureConsumers = null)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddEntityFrameworkOutbox<AppDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                configureConsumers?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration.GetConnectionString("RabbitMq")
                        ?? "amqp://guest:guest@localhost:5672";
                    cfg.Host(new Uri(host));
                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IMessageBus, MassTransitMessageBus>();
            return services;
        }
    }
}
