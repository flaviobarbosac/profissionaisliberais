using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class LancamentoFinanceiroDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid? ClienteId { get; set; }
        public Guid? CobrancaId { get; set; }
        public TipoLancamento Tipo { get; set; }
        public StatusLancamento Status { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public decimal Valor { get; set; }
        public DateTime Vencimento { get; set; }
        public DateTime? PagoEm { get; set; }
        public string? ComprovanteUrl { get; set; }
        public string? Fornecedor { get; set; }
    }
}
