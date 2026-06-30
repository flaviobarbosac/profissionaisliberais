using System.ComponentModel.DataAnnotations;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Cliente (pagador) de um prestador de serviço.
    /// </summary>
    public class Cliente : ModelBase, ITenantOwned
    {
        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [MaxLength(5)]
        public string? Ddd { get; set; }

        [MaxLength(18)]
        public string? CpfCnpj { get; set; }

        // Identificador do pagador na subconta Asaas
        [MaxLength(255)]
        public string? AsaasCustomerId { get; set; }

        // Endereço
        [MaxLength(10)]
        public string? Cep { get; set; }

        [MaxLength(200)]
        public string? Logradouro { get; set; }

        [MaxLength(20)]
        public string? Numero { get; set; }

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [MaxLength(100)]
        public string? Bairro { get; set; }

        [MaxLength(100)]
        public string? Cidade { get; set; }

        [MaxLength(2)]
        public string? Uf { get; set; }
    }
}
