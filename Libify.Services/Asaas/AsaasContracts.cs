using Libify.Domain.Model;

namespace Libify.Services.Asaas
{
  public record EmitirCobrancaRequest(
    Guid ClienteId,
    decimal Valor,
    DateTime Vencimento,
    Domain.Enum.FormaPagamento FormaPagamento,
    Guid? PropostaId = null,
    int Parcelas = 1,
    string? Descricao = null);

  public record AssinarPlanoRequest(
    Domain.Enum.TipoPlano Tipo,
    Domain.Enum.FormaPagamento FormaPagamento = Domain.Enum.FormaPagamento.Pix);

  public interface IContaAsaasService
  {
    Task<Usuario> CriarSubcontaAsync(CancellationToken cancellationToken = default);
    Task<Usuario> ConsultarStatusAsync(CancellationToken cancellationToken = default);
  }

  public interface ICobrancaAsaasService
  {
    Task<Cobranca> EmitirAsync(EmitirCobrancaRequest request, CancellationToken cancellationToken = default);
    Task CancelarAsync(Guid cobrancaId, CancellationToken cancellationToken = default);
  }

  public interface IPlanoAssinaturaService
  {
    Task<Plano> AssinarAsync(AssinarPlanoRequest request, CancellationToken cancellationToken = default);
    Task CancelarAsync(CancellationToken cancellationToken = default);
  }

  public interface IAsaasWebhookProcessor
  {
    Task<string> ProcessarAsync(string payload, string? authToken, CancellationToken cancellationToken = default);
  }
}
