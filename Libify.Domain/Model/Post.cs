using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Post de portfólio (antes/depois) com legenda gerada pelo Gemini e publicação no Instagram.
    /// </summary>
    public class Post : ModelBase
    {
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [MaxLength(500)]
        public string? FotoAntesUrl { get; set; }

        [MaxLength(500)]
        public string? FotoDepoisUrl { get; set; }

        [MaxLength(2200)]
        public string? Legenda { get; set; }

        [MaxLength(500)]
        public string? Hashtags { get; set; }

        public bool Publicado { get; set; }
        public DateTime? PublicadoEm { get; set; }

        [MaxLength(255)]
        public string? InstagramMediaId { get; set; }
    }
}
