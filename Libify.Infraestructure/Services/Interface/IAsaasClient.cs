using Libify.Infraestructure.Services;

namespace Libify.Infraestructure.Services.Interface
{
  /// <summary>
  /// Proxy de acesso à API do Asaas (white-label). A chave da subconta (apiKey do usuário)
  /// é passada por chamada para garantir o isolamento de dados entre os prestadores.
  /// </summary>
  public interface IAsaasClient
  {
    Task<AsaasContaResponse> CriarSubcontaAsync(AsaasSubcontaRequest dadosConta, CancellationToken cancellationToken = default);
    Task<AsaasContaResponse> ObterContaAsync(string apiKeySubconta, string accountId, CancellationToken cancellationToken = default);

    Task<AsaasClienteResponse> CriarClienteAsync(string apiKey, AsaasClienteRequest cliente, CancellationToken cancellationToken = default);
    Task<AsaasClienteResponse> AtualizarClienteAsync(string apiKey, string customerId, AsaasClienteRequest cliente, CancellationToken cancellationToken = default);

    Task<AsaasCobrancaResponse> CriarCobrancaAsync(string apiKey, AsaasCobrancaRequest cobranca, CancellationToken cancellationToken = default);
    Task<AsaasCobrancaResponse> ObterCobrancaAsync(string apiKey, string paymentId, CancellationToken cancellationToken = default);
    Task CancelarCobrancaAsync(string apiKey, string paymentId, CancellationToken cancellationToken = default);
    Task<AsaasPixQrCodeResponse> ObterPixQrCodeAsync(string apiKey, string paymentId, CancellationToken cancellationToken = default);

    Task<AsaasAssinaturaResponse> CriarAssinaturaAsync(string apiKey, AsaasAssinaturaRequest assinatura, CancellationToken cancellationToken = default);
    Task CancelarAssinaturaAsync(string apiKey, string subscriptionId, CancellationToken cancellationToken = default);

    Task<AsaasLinkPagamentoResponse> CriarLinkPagamentoAsync(string apiKey, AsaasLinkPagamentoRequest linkPagamento, CancellationToken cancellationToken = default);
  }
}
