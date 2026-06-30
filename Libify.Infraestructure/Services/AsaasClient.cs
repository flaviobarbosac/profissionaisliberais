using System.Net.Http.Headers;
using Libify.Infraestructure.Services.Interface;

namespace Libify.Infraestructure.Services
{
    /// <summary>
    /// Implementação base do client Asaas. Os métodos abaixo são o ponto de integração
    /// (esqueleto) — a serialização de payloads e o parse das respostas serão completados
    /// conforme cada fluxo (cobrança, subconta, checkout) for implementado.
    /// </summary>
    public class AsaasClient : IAsaasClient
    {
        private readonly HttpClient _httpClient;
        private readonly AsaasConfig _config;

        public AsaasClient(HttpClient httpClient, AsaasConfig config)
        {
            _httpClient = httpClient;
            _config = config;
            _httpClient.BaseAddress = new Uri(_config.ApiUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        }

        public Task<AsaasContaResponse> CriarSubcontaAsync(AsaasSubcontaRequest dadosConta, CancellationToken cancellationToken = default)
            => EnviarAsync<AsaasContaResponse>(_config.PlatformApiKey, HttpMethod.Post, "/accounts", dadosConta, cancellationToken);

        public Task<AsaasClienteResponse> CriarClienteAsync(string apiKeySubconta, AsaasClienteRequest cliente, CancellationToken cancellationToken = default)
            => EnviarAsync<AsaasClienteResponse>(apiKeySubconta, HttpMethod.Post, "/customers", cliente, cancellationToken);

        public Task<AsaasCobrancaResponse> CriarCobrancaAsync(string apiKeySubconta, AsaasCobrancaRequest cobranca, CancellationToken cancellationToken = default)
            => EnviarAsync<AsaasCobrancaResponse>(apiKeySubconta, HttpMethod.Post, "/payments", cobranca, cancellationToken);

        public Task<AsaasPixQrCodeResponse> ObterPixQrCodeAsync(string apiKeySubconta, string paymentId, CancellationToken cancellationToken = default)
            => EnviarAsync<AsaasPixQrCodeResponse>(apiKeySubconta, HttpMethod.Get, $"/payments/{paymentId}/pixQrCode", null, cancellationToken);

        public Task<AsaasLinkPagamentoResponse> CriarLinkPagamentoAsync(string apiKeySubconta, AsaasLinkPagamentoRequest linkPagamento, CancellationToken cancellationToken = default)
            => EnviarAsync<AsaasLinkPagamentoResponse>(apiKeySubconta, HttpMethod.Post, "/paymentLinks", linkPagamento, cancellationToken);

        private async Task<TResponse> EnviarAsync<TResponse>(string apiKey, HttpMethod metodo, string rota, object? corpo, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(metodo, rota);
            request.Headers.Add("access_token", apiKey);

            if (corpo is not null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(corpo);
                request.Content = new StringContent(json);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(body)
                ?? throw new InvalidOperationException("Resposta vazia do Asaas.");
        }
    }
}
