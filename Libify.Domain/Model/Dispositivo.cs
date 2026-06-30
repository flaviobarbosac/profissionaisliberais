using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Dispositivo/sessão vinculado à conta do usuário. Permite multi-device e
    /// que o usuário liste e desconecte dispositivos.
    /// </summary>
    public class Dispositivo : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        /// <summary>Identificador estável gerado pelo cliente (instalação do app/navegador).</summary>
        [Required]
        [MaxLength(200)]
        public string DeviceId { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Nome { get; set; }

        [MaxLength(50)]
        public string? Plataforma { get; set; }

        [MaxLength(50)]
        public string? AppVersion { get; set; }

        [MaxLength(45)]
        public string? Ip { get; set; }

        [MaxLength(300)]
        public string? UserAgent { get; set; }

        public DateTime? UltimoAcessoEm { get; set; }
        public DateTime? UltimaSyncEm { get; set; }

        public bool Revogado { get; set; }
        public DateTime? RevogadoEm { get; set; }
    }
}
