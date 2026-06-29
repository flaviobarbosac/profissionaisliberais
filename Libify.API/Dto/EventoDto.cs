using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class EventoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? ClienteId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public StatusEvento Status { get; set; }
        public string? Local { get; set; }
        public string? MeetUrl { get; set; }
    }
}
