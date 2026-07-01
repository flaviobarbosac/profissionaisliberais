using Asp.Versioning;
using Libify.Services.Asaas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/webhooks/asaas")]
  [AllowAnonymous]
  public class AsaasWebhookController : ControllerBase
  {
    private readonly IAsaasWebhookProcessor _processor;

    public AsaasWebhookController(IAsaasWebhookProcessor processor) => _processor = processor;

    [HttpPost]
    public async Task<IActionResult> Receber(CancellationToken ct)
    {
      using var reader = new StreamReader(Request.Body);
      var payload = await reader.ReadToEndAsync(ct);
      if (string.IsNullOrWhiteSpace(payload))
        return BadRequest();

      Request.Headers.TryGetValue("asaas-access-token", out var token);

      try
      {
        var resultado = await _processor.ProcessarAsync(payload, token.FirstOrDefault(), ct);
        return Ok(new { resultado });
      }
      catch (UnauthorizedAccessException)
      {
        return Unauthorized();
      }
    }
  }
}
