using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class NotaFiscalDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int ClienteId { get; set; }
        public int? ContratoId { get; set; }
        public int? CobrancaId { get; set; }
        public StatusNotaFiscal Status { get; set; }
        public decimal Valor { get; set; }
        public string? DiscriminacaoServico { get; set; }
        public string? Numero { get; set; }
        public string? PdfUrl { get; set; }
        public DateTime? EmitidaEm { get; set; }
    }
}
