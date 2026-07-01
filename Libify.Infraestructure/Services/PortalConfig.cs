namespace Libify.Infraestructure.Services
{
  public class PortalConfig
  {
    /// <summary>URL base do front (ex.: https://app.libify.com.br).</summary>
    public string FrontBaseUrl { get; set; } = string.Empty;

    /// <summary>Validade padrão do link público após envio/renovação.</summary>
    public int LinkValidadeDias { get; set; } = 30;
  }
}
