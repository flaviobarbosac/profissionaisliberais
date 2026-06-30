namespace Libify.API.Dto
{
    public class TarefaDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool Concluida { get; set; }
        public DateTime? Vencimento { get; set; }
    }
}
