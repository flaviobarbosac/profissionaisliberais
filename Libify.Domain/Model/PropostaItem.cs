using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Item (serviço marcado) de uma proposta, com quantidade e valor.
    /// </summary>
    public class PropostaItem : ModelBase
    {
        [Required]
        public int PropostaId { get; set; }
        public Proposta Proposta { get; set; } = null!;

        public int? ServicoId { get; set; }
        public Servico? Servico { get; set; }

        [Required]
        [MaxLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantidade { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
    }
}
