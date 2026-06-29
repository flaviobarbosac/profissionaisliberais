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
    }
}
