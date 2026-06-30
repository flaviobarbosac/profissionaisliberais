using Libify.Domain.Model;
using Libify.Infraestructure.Configuration;
using Libify.Infraestructure.Messaging;
using Libify.Infraestructure.Observability;
using Libify.Worker.Consumers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.Enrich.FromLogContext().WriteTo.Console();
    if (!context.HostingEnvironment.IsDevelopment())
        configuration.WriteTo.OpenTelemetry();
});

// Observabilidade (traces + métricas + OTLP) e métricas de negócio
builder.Services.AddLibifyObservability("Libify.Worker");
builder.Services.AddMetrics();
builder.Services.AddSingleton<LibifyMetrics>();

builder.Services.AddLibifyPersistence(builder.Configuration);

// Mensageria com um consumidor (fila) por módulo
builder.Services.AddLibifyMessaging(builder.Configuration, x =>
{
    x.AddConsumer<SyncConsumer<Cliente>>();
    x.AddConsumer<SyncConsumer<Servico>>();
    x.AddConsumer<SyncConsumer<Proposta>>();
    x.AddConsumer<SyncConsumer<PropostaItem>>();
    x.AddConsumer<SyncConsumer<Contrato>>();
    x.AddConsumer<SyncConsumer<Cobranca>>();
    x.AddConsumer<SyncConsumer<LancamentoFinanceiro>>();
    x.AddConsumer<SyncConsumer<NotaFiscal>>();
    x.AddConsumer<SyncConsumer<Evento>>();
    x.AddConsumer<SyncConsumer<Tarefa>>();
    x.AddConsumer<SyncConsumer<Post>>();
    x.AddConsumer<SyncConsumer<Plano>>();
});

// Health check (inclui o bus MassTransit, registrado automaticamente)
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
