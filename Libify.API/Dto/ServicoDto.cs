namespace Libify.API.Dto
{
    public class ServicoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public string? Unidade { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
