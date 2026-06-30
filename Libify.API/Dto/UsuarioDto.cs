using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? CpfCnpj { get; set; }
        public CategoriaProfissional Categoria { get; set; }
        public TipoPlano Plano { get; set; }
        public string Locale { get; set; } = "pt-BR";
        public string Pais { get; set; } = "BR";
    }
}
