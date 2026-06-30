using System.Diagnostics.CodeAnalysis;
using Libify.Infraestructure.Observability;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Libify.Infraestructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ObservabilityExtensions
    {
        /// <summary>
        /// Configura a base de OpenTelemetry (traces + métricas) comum a API e Worker:
        /// recurso, propagação pelo MassTransit, HttpClient, runtime, meter de negócio e OTLP.
        /// Instrumentação específica de ASP.NET Core é adicionada pela API separadamente.
        /// </summary>
        public static IServiceCollection AddLibifyObservability(this IServiceCollection services, string serviceName)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService(serviceName))
                .WithTracing(t => t
                    .AddSource("MassTransit")
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter())
                .WithMetrics(m => m
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(LibifyMetrics.MeterName)
                    .AddOtlpExporter());

            return services;
        }
    }
}
