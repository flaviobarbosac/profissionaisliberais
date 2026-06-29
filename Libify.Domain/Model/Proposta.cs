using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Proposta comercial enviada a um cliente (rascunho/enviada/aceita/recusada).
    /// </summary>
    public class Proposta : ModelBase
    {
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        [MaxLength(200)]
        public string? Titulo { get; set; }

        public PropostaStatus Status { get; set; } = PropostaStatus.Rascunho;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        [MaxLength(2000)]
        public string? Observacoes { get; set; }

        // Documento gerado (PDF) armazenado no Google Drive do usuário
        [MaxLength(500)]
        public string? DriveFileId { get; set; }

        public DateTime? EnviadaEm { get; set; }
        public DateTime? RespondidaEm { get; set; }

        public ICollection<PropostaItem> Itens { get; set; } = new List<PropostaItem>();
    }
}
