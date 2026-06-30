using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Cobrança (PIX / Boleto / Cartão) criada no Asaas para um cliente.
    /// </summary>
    public class Cobranca : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public Guid? PropostaId { get; set; }
        public Proposta? Proposta { get; set; }

        public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;
        public StatusCobranca Status { get; set; } = StatusCobranca.Pendente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public DateTime Vencimento { get; set; }

        public int Parcelas { get; set; } = 1;

        [MaxLength(255)]
        public string? Descricao { get; set; }

        // Referências Asaas
        [MaxLength(255)]
        public string? AsaasPaymentId { get; set; }

        [MaxLength(1000)]
        public string? PixQrCode { get; set; }

        [MaxLength(500)]
        public string? PixCopiaECola { get; set; }

        [MaxLength(500)]
        public string? BoletoUrl { get; set; }

        [MaxLength(500)]
        public string? LinkPagamento { get; set; }
    }
}
