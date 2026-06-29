using System.ComponentModel.DataAnnotations;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Compromisso na agenda (sincronizado com Google Agenda / Meet / Maps).
    /// </summary>
    public class Evento : ModelBase
    {
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }

        public StatusEvento Status { get; set; } = StatusEvento.Agendado;

        [MaxLength(300)]
        public string? Local { get; set; }

        [MaxLength(500)]
        public string? MeetUrl { get; set; }

        // Identificador do evento no Google Calendar do usuário
        [MaxLength(255)]
        public string? GoogleEventId { get; set; }
    }
}
