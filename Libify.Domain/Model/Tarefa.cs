using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Tarefa / follow-up (sincroniza com Google Tasks / Keep).
    /// </summary>
    public class Tarefa : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        public bool Concluida { get; set; }
        public DateTime? Vencimento { get; set; }

        [MaxLength(255)]
        public string? GoogleTaskId { get; set; }
    }
}
