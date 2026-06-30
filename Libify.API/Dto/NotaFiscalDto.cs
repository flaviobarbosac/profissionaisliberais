using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class NotaFiscalDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid ClienteId { get; set; }
        public Guid? ContratoId { get; set; }
        public Guid? CobrancaId { get; set; }
        public StatusNotaFiscal Status { get; set; }
        public decimal Valor { get; set; }
        public string? DiscriminacaoServico { get; set; }
        public string? Numero { get; set; }
        public string? PdfUrl { get; set; }
        public DateTime? EmitidaEm { get; set; }
    }
}
