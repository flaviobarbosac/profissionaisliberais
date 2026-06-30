using Libify.Domain.Messaging;
using Libify.Domain.Model;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Observability;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Libify.Worker.Consumers
{
    /// <summary>
    /// Consumidor genérico de sincronização. Por ser fechado por tipo (um por módulo),
    /// o MassTransit cria uma fila/endpoint dedicada por módulo. Atualiza a marca d'água
    /// de sincronização do lado servidor (web).
    /// </summary>
    public class SyncConsumer<T> : IConsumer<SyncEvent<T>> where T : class
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SyncConsumer<T>> _logger;
        private readonly LibifyMetrics _metrics;

        public SyncConsumer(AppDbContext db, ILogger<SyncConsumer<T>> logger, LibifyMetrics metrics)
        {
            _db = db;
            _logger = logger;
            _metrics = metrics;
        }

        public async Task Consume(ConsumeContext<SyncEvent<T>> context)
        {
            var msg = context.Message;
            var modulo = typeof(T).Name;

            _logger.LogInformation(
                "Sync recebido: {Operacao} {Modulo} entidade {EntityId} v{Version} tenant {Tenant}",
                msg.Operacao, modulo, msg.EntityId, msg.Version, msg.UsuarioId);

            var agora = DateTime.UtcNow;
            var watermark = await _db.SyncWatermark.IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.UsuarioId == msg.UsuarioId
                    && w.DispositivoId == Guid.Empty
                    && w.Modulo == modulo);

            if (watermark is null)
            {
                _db.SyncWatermark.Add(new SyncWatermark
                {
                    Id = Guid.CreateVersion7(),
                    UsuarioId = msg.UsuarioId,
                    DispositivoId = Guid.Empty,
                    Modulo = modulo,
                    UltimoSyncEm = agora,
                    CreatedAt = agora,
                    UpdatedAt = agora,
                    Version = 1
                });
            }
            else
            {
                watermark.UltimoSyncEm = agora;
                watermark.UpdatedAt = agora;
                watermark.Version += 1;
            }

            await _db.SaveChangesAsync();
            _metrics.SyncProcessado(modulo, msg.Operacao.ToString());
        }
    }
}
