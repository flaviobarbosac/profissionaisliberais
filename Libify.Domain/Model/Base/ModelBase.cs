using System.ComponentModel.DataAnnotations;

namespace Libify.Domain.Model.Base
{
    public class ModelBase
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
