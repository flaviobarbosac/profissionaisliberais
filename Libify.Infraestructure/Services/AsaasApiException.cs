using System.Net;

namespace Libify.Infraestructure.Services
{
  public class AsaasApiException : Exception
  {
    public HttpStatusCode StatusCode { get; }
    public IReadOnlyList<string> Errors { get; }

    public AsaasApiException(HttpStatusCode statusCode, IReadOnlyList<string> errors, string? body = null)
      : base(errors.Count > 0 ? string.Join("; ", errors) : $"Erro Asaas HTTP {(int)statusCode}")
    {
      StatusCode = statusCode;
      Errors = errors;
    }
  }
}
