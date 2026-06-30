using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class PropostaDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid ClienteId { get; set; }
        public string? Titulo { get; set; }
        public PropostaStatus Status { get; set; }
        public decimal ValorTotal { get; set; }
        public string? Observacoes { get; set; }
        public List<PropostaItemDto> Itens { get; set; } = new();
    }

    public class PropostaItemDto
    {
        public Guid Id { get; set; }
        public Guid PropostaId { get; set; }
        public Guid? ServicoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Quantidade { get; set; } = 1;
        public decimal ValorUnitario { get; set; }
        public decimal Total { get; set; }
    }
}
