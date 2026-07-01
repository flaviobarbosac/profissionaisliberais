namespace Libify.Services.Proposta
{
  public record PropostaLinkResult(string Token, string Url, DateTime? ExpiraEm);

  public record PropostaPortalFechamentoView(
    Guid? ContratoId,
    Guid? CobrancaId,
    string? LinkPagamento,
    string? PixCopiaECola,
    bool CobrancaAsaasEmitida,
    string? AvisoAsaas);

  public record PropostaPortalView(
    Guid Id,
    string ProfissionalNome,
    string ClienteNome,
    string? Titulo,
    int Status,
    decimal ValorTotal,
    string? Observacoes,
    DateTime? EnviadaEm,
    DateTime? LinkExpiraEm,
    bool PodeResponder,
    IReadOnlyList<PropostaPortalItemView> Itens,
    PropostaPortalFechamentoView? Fechamento = null);

  public record PropostaPortalItemView(
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal Total);

  public interface IPropostaPortalService
  {
    Task<PropostaLinkResult> EnviarAsync(Guid propostaId, CancellationToken cancellationToken = default);
    Task<PropostaLinkResult> ObterOuGerarLinkAsync(Guid propostaId, CancellationToken cancellationToken = default);
    Task<PropostaPortalView> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<PropostaPortalView> AceitarAsync(string token, string? respondidoPor, string? ipAceite = null, CancellationToken cancellationToken = default);
    Task<PropostaPortalView> RecusarAsync(string token, string? respondidoPor, string? motivo, CancellationToken cancellationToken = default);
  }
}
