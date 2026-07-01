using System.ComponentModel.DataAnnotations;
using Libify.Domain.Enum;
using Libify.Domain.Model.Base;

namespace Libify.Domain.Model
{
    /// <summary>
    /// Prestador de serviço / profissional liberal / MEI. Raiz do isolamento de dados (tenant).
    /// </summary>
    public class Usuario : ModelBase
    {
        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        public bool TelefoneVerificado { get; set; }

        [MaxLength(18)]
        public string? CpfCnpj { get; set; }

        public CategoriaProfissional Categoria { get; set; } = CategoriaProfissional.Outro;

        // Login social (Google OAuth)
        [MaxLength(255)]
        public string? GoogleId { get; set; }

        [MaxLength(500)]
        public string? FotoUrl { get; set; }

        // Dados fiscais (NFS-e)
        [MaxLength(7)]
        public string? Cnae { get; set; }

        [MaxLength(100)]
        public string? Municipio { get; set; }

        [MaxLength(2)]
        public string? Uf { get; set; }

        // Plano / monetização
        public TipoPlano Plano { get; set; } = TipoPlano.Gratuito;
        public DateTime? PlanoValidoAte { get; set; }

        // Subconta white-label Asaas
        [MaxLength(255)]
        public string? AsaasAccountId { get; set; }

    [MaxLength(512)]
    public string? AsaasApiKey { get; set; }

        /// <summary>Customer na conta plataforma Libify (assinatura Premium).</summary>
        [MaxLength(255)]
        public string? AsaasPlatformCustomerId { get; set; }

        public StatusContaAsaas StatusContaAsaas { get; set; } = StatusContaAsaas.Pendente;

        // Locale (i18n / expansão internacional)
        [MaxLength(5)]
        public string Locale { get; set; } = "pt-BR";

        [MaxLength(2)]
        public string Pais { get; set; } = "BR";
    }
}
