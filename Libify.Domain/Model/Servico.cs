using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Item do catálogo de serviços do prestador (preço padrão).
    /// </summary>
    public class Servico : ModelBase
    {
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Descricao { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        [MaxLength(20)]
        public string? Unidade { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
