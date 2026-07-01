namespace Libify.API.Dto
{
  public class PropostaLinkDto
  {
    public string Token { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime? ExpiraEm { get; set; }
  }

  public class PropostaPortalFechamentoDto
  {
    public Guid? ContratoId { get; set; }
    public Guid? CobrancaId { get; set; }
    public string? LinkPagamento { get; set; }
    public string? PixCopiaECola { get; set; }
    public bool CobrancaAsaasEmitida { get; set; }
    public string? AvisoAsaas { get; set; }
  }

  public class PropostaPortalDto
  {
    public Guid Id { get; set; }
    public string ProfissionalNome { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public int Status { get; set; }
    public decimal ValorTotal { get; set; }
    public string? Observacoes { get; set; }
    public DateTime? EnviadaEm { get; set; }
    public DateTime? LinkExpiraEm { get; set; }
    public bool PodeResponder { get; set; }
    public List<PropostaPortalItemDto> Itens { get; set; } = new();
    public PropostaPortalFechamentoDto? Fechamento { get; set; }
  }

  public class PropostaPortalItemDto
  {
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Total { get; set; }
  }

  public class ResponderPropostaPortalDto
  {
    public string? Nome { get; set; }
  }

  public class RecusarPropostaPortalDto
  {
    public string? Nome { get; set; }
    public string? Motivo { get; set; }
  }
}
