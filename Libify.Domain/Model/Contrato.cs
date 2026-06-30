using System.ComponentModel.DataAnnotations;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Contrato gerado a partir de uma proposta aceita, com aceite/assinatura digital.
    /// </summary>
    public class Contrato : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public Guid PropostaId { get; set; }
        public Proposta Proposta { get; set; } = null!;

        public ContratoStatus Status { get; set; } = ContratoStatus.Pendente;

        [MaxLength(500)]
        public string? DriveFileId { get; set; }

        // Aceite / assinatura digital
        public bool Aceito { get; set; }
        public DateTime? AceitoEm { get; set; }

        [MaxLength(150)]
        public string? AceitoPor { get; set; }

        [MaxLength(45)]
        public string? IpAceite { get; set; }
    }
}
