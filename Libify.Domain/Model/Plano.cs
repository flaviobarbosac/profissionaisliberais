using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Assinatura Premium do usuário (mensal/semestral/anual), cobrada de forma recorrente via Asaas.
    /// </summary>
    public class Plano : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public TipoPlano Tipo { get; set; } = TipoPlano.Mensal;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public DateTime InicioEm { get; set; }
        public DateTime? FimEm { get; set; }

        public bool Ativo { get; set; } = true;

        // Assinatura recorrente Asaas
        [MaxLength(255)]
        public string? AsaasSubscriptionId { get; set; }
    }
}
