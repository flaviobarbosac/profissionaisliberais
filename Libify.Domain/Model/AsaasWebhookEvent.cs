using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Registro idempotente de eventos recebidos do webhook Asaas (sem tenant — correlaciona por IDs Asaas).
    /// </summary>
    public class AsaasWebhookEvent : ModelBase
    {
        [Required]
        [MaxLength(120)]
        public string EventId { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public string Payload { get; set; } = string.Empty;

        public DateTime? ProcessadoEm { get; set; }

        [MaxLength(50)]
        public string? Resultado { get; set; }
    }
}
