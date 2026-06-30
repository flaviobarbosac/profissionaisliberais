using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Marca d'água de sincronização por dispositivo e módulo (delta sync incremental).
    /// </summary>
    public class SyncWatermark : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }

        [Required]
        public Guid DispositivoId { get; set; }

        [Required]
        [MaxLength(80)]
        public string Modulo { get; set; } = string.Empty;

        public DateTime UltimoSyncEm { get; set; }
    }
}
