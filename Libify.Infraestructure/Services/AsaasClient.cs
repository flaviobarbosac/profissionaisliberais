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

        public Task<string> CriarSubcontaAsync(object dadosConta, CancellationToken cancellationToken = default)
            => EnviarAsync(_config.PlatformApiKey, HttpMethod.Post, "/accounts", dadosConta, cancellationToken);

        public Task<string> CriarClienteAsync(string apiKeySubconta, object cliente, CancellationToken cancellationToken = default)
            => EnviarAsync(apiKeySubconta, HttpMethod.Post, "/customers", cliente, cancellationToken);

        public Task<string> CriarCobrancaAsync(string apiKeySubconta, object cobranca, CancellationToken cancellationToken = default)
            => EnviarAsync(apiKeySubconta, HttpMethod.Post, "/payments", cobranca, cancellationToken);

        public Task<string> ObterPixQrCodeAsync(string apiKeySubconta, string paymentId, CancellationToken cancellationToken = default)
            => EnviarAsync(apiKeySubconta, HttpMethod.Get, $"/payments/{paymentId}/pixQrCode", null, cancellationToken);

        public Task<string> CriarLinkPagamentoAsync(string apiKeySubconta, object linkPagamento, CancellationToken cancellationToken = default)
            => EnviarAsync(apiKeySubconta, HttpMethod.Post, "/paymentLinks", linkPagamento, cancellationToken);

        private async Task<string> EnviarAsync(string apiKey, HttpMethod metodo, string rota, object? corpo, CancellationToken cancellationToken)
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
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
