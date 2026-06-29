using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Lançamento de contas a pagar ou a receber (financeiro).
    /// Despesas podem ser lançadas por OCR (foto do comprovante via Gemini).
    /// </summary>
    public class LancamentoFinanceiro : ModelBase
    {
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int? CobrancaId { get; set; }
        public Cobranca? Cobranca { get; set; }

        public TipoLancamento Tipo { get; set; } = TipoLancamento.Pagar;
        public StatusLancamento Status { get; set; } = StatusLancamento.Pendente;

        [Required]
        [MaxLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Categoria { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public DateTime Vencimento { get; set; }
        public DateTime? PagoEm { get; set; }

        // Comprovante (foto) armazenado no Drive
        [MaxLength(500)]
        public string? ComprovanteUrl { get; set; }

        [MaxLength(150)]
        public string? Fornecedor { get; set; }
    }
}
