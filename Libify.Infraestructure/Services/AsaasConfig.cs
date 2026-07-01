namespace Libify.Infraestructure.Services
{
  /// <summary>
  /// Configuração da integração white-label com o Asaas.
  /// </summary>
  public class AsaasConfig
  {
    public string ApiUrl { get; set; } = "https://sandbox.asaas.com/api/v3";
    public string PlatformApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Token validado no header asaas-access-token dos webhooks.</summary>
    public string WebhookAuthToken { get; set; } = string.Empty;

    /// <summary>URL base pública para registrar webhooks nas subcontas (ex.: https://api.libify.com.br).</summary>
    public string? WebhookBaseUrl { get; set; }

    /// <summary>Chave AES para criptografar AsaasApiKey em repouso (mín. 32 caracteres).</summary>
    public string? EncryptionKey { get; set; }

    public decimal ValorPlanoMensal { get; set; } = 29.90m;
    public decimal ValorPlanoSemestral { get; set; } = 149.90m;
    public decimal ValorPlanoAnual { get; set; } = 249.90m;
  }
}
