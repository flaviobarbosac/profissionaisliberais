using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class PlanoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public TipoPlano Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime InicioEm { get; set; }
        public DateTime? FimEm { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
