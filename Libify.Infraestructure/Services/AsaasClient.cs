using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Libify.Infraestructure.Services.Interface;

namespace Libify.Infraestructure.Services
{
  public class AsaasClient : IAsaasClient
  {
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly AsaasConfig _config;

    public AsaasClient(HttpClient httpClient, AsaasConfig config)
    {
      _httpClient = httpClient;
      _config = config;
      _httpClient.BaseAddress = new Uri(_config.ApiUrl.TrimEnd('/') + "/");
      _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
    }

    public Task<AsaasContaResponse> CriarSubcontaAsync(AsaasSubcontaRequest dadosConta, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasContaResponse>(_config.PlatformApiKey, HttpMethod.Post, "accounts", dadosConta, cancellationToken);

    public Task<AsaasContaResponse> ObterContaAsync(string apiKeySubconta, string accountId, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasContaResponse>(apiKeySubconta, HttpMethod.Get, $"accounts/{accountId}", null, cancellationToken);

    public Task<AsaasClienteResponse> CriarClienteAsync(string apiKey, AsaasClienteRequest cliente, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasClienteResponse>(apiKey, HttpMethod.Post, "customers", cliente, cancellationToken);

    public Task<AsaasClienteResponse> AtualizarClienteAsync(string apiKey, string customerId, AsaasClienteRequest cliente, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasClienteResponse>(apiKey, HttpMethod.Put, $"customers/{customerId}", cliente, cancellationToken);

    public Task<AsaasCobrancaResponse> CriarCobrancaAsync(string apiKey, AsaasCobrancaRequest cobranca, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasCobrancaResponse>(apiKey, HttpMethod.Post, "payments", cobranca, cancellationToken);

    public Task<AsaasCobrancaResponse> ObterCobrancaAsync(string apiKey, string paymentId, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasCobrancaResponse>(apiKey, HttpMethod.Get, $"payments/{paymentId}", null, cancellationToken);

    public Task CancelarCobrancaAsync(string apiKey, string paymentId, CancellationToken cancellationToken = default)
      => EnviarAsync<object?>(apiKey, HttpMethod.Delete, $"payments/{paymentId}", null, cancellationToken);

    public Task<AsaasPixQrCodeResponse> ObterPixQrCodeAsync(string apiKey, string paymentId, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasPixQrCodeResponse>(apiKey, HttpMethod.Get, $"payments/{paymentId}/pixQrCode", null, cancellationToken);

    public Task<AsaasAssinaturaResponse> CriarAssinaturaAsync(string apiKey, AsaasAssinaturaRequest assinatura, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasAssinaturaResponse>(apiKey, HttpMethod.Post, "subscriptions", assinatura, cancellationToken);

    public Task CancelarAssinaturaAsync(string apiKey, string subscriptionId, CancellationToken cancellationToken = default)
      => EnviarAsync<object?>(apiKey, HttpMethod.Delete, $"subscriptions/{subscriptionId}", null, cancellationToken);

    public Task<AsaasLinkPagamentoResponse> CriarLinkPagamentoAsync(string apiKey, AsaasLinkPagamentoRequest linkPagamento, CancellationToken cancellationToken = default)
      => EnviarAsync<AsaasLinkPagamentoResponse>(apiKey, HttpMethod.Post, "paymentLinks", linkPagamento, cancellationToken);

    private async Task<TResponse> EnviarAsync<TResponse>(string apiKey, HttpMethod metodo, string rota, object? corpo, CancellationToken cancellationToken)
    {
      using var request = new HttpRequestMessage(metodo, rota);
      request.Headers.Add("access_token", apiKey);
      request.Headers.Add("User-Agent", "Libify/1.0");

      if (corpo is not null)
      {
        var json = JsonSerializer.Serialize(corpo, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      }

      var response = await _httpClient.SendAsync(request, cancellationToken);
      var body = await response.Content.ReadAsStringAsync(cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var erros = ExtrairErros(body);
        throw new AsaasApiException(response.StatusCode, erros, body);
      }

      if (typeof(TResponse) == typeof(object) || string.IsNullOrWhiteSpace(body))
        return default!;

      return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)
        ?? throw new InvalidOperationException("Resposta vazia do Asaas.");
    }

    private static IReadOnlyList<string> ExtrairErros(string body)
    {
      try
      {
        var parsed = JsonSerializer.Deserialize<AsaasErrorResponse>(body);
        if (parsed?.Errors is { Length: > 0 })
          return parsed.Errors
            .Select(e => string.IsNullOrWhiteSpace(e.Description) ? e.Code ?? "erro" : e.Description!)
            .ToList();
      }
      catch
      {
        // corpo não-JSON
      }

      return new[] { body.Length > 200 ? body[..200] : body };
    }
  }
}
