using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Nota Fiscal de Serviço (NFS-e Nacional / fallback eNotas).
    /// </summary>
    public class NotaFiscal : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public Guid? ContratoId { get; set; }
        public Contrato? Contrato { get; set; }

        public Guid? CobrancaId { get; set; }
        public Cobranca? Cobranca { get; set; }

        public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Pendente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [MaxLength(2000)]
        public string? DiscriminacaoServico { get; set; }

        [MaxLength(50)]
        public string? Numero { get; set; }

        [MaxLength(500)]
        public string? PdfUrl { get; set; }

        [MaxLength(500)]
        public string? XmlUrl { get; set; }

        public DateTime? EmitidaEm { get; set; }
    }
}
