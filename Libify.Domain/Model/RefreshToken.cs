using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Refresh token (armazenado como hash) por dispositivo. Rotacionado a cada uso.
    /// </summary>
    public class RefreshToken : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public Guid DispositivoId { get; set; }
        public Dispositivo Dispositivo { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiraEm { get; set; }

        public bool Revogado { get; set; }
        public DateTime? RevogadoEm { get; set; }

        /// <summary>Hash do token que substituiu este (rotação), para auditoria.</summary>
        [MaxLength(200)]
        public string? SubstituidoPorHash { get; set; }
    }
}
