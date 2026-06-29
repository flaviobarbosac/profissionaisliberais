using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class ContratoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PropostaId { get; set; }
        public ContratoStatus Status { get; set; }
        public bool Aceito { get; set; }
        public DateTime? AceitoEm { get; set; }
        public string? AceitoPor { get; set; }
    }
}
