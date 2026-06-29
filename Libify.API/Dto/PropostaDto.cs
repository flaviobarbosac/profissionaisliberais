using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class PropostaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int ClienteId { get; set; }
        public string? Titulo { get; set; }
        public PropostaStatus Status { get; set; }
        public decimal ValorTotal { get; set; }
        public string? Observacoes { get; set; }
        public List<PropostaItemDto> Itens { get; set; } = new();
    }

    public class PropostaItemDto
    {
        public int Id { get; set; }
        public int PropostaId { get; set; }
        public int? ServicoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Quantidade { get; set; } = 1;
        public decimal ValorUnitario { get; set; }
        public decimal Total { get; set; }
    }
}
