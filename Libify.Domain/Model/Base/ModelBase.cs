using System.ComponentModel.DataAnnotations;

namespace Libify.Domain.Model.Base
{
    /// <summary>
    /// Base de todas as entidades. Id em Guid v7 (gerado no cliente, pronto para sync offline),
    /// Version para resolução de conflito (Last-Write-Wins) e DeletedAt como tombstone (soft delete).
    /// </summary>
    public abstract class ModelBase
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>Incrementa a cada atualização; base do LWW na sincronização.</summary>
        public long Version { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>Tombstone: quando preenchido, o registro foi excluído logicamente.</summary>
        public DateTime? DeletedAt { get; set; }
    }
}
