using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class ContratoDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid PropostaId { get; set; }
        public ContratoStatus Status { get; set; }
        public bool Aceito { get; set; }
        public DateTime? AceitoEm { get; set; }
        public string? AceitoPor { get; set; }
    }
}
