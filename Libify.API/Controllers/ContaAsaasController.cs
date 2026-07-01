using Asp.Versioning;
using Libify.Services.Asaas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/conta-asaas")]
  [Authorize]
  public class ContaAsaasController : ControllerBase
  {
    private readonly IContaAsaasService _conta;

    public ContaAsaasController(IContaAsaasService conta) => _conta = conta;

    [HttpPost("criar")]
    public async Task<IActionResult> Criar(CancellationToken ct)
    {
      try
      {
        var usuario = await _conta.CriarSubcontaAsync(ct);
        return Ok(new { usuario.AsaasAccountId, usuario.StatusContaAsaas });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
      try
      {
        var usuario = await _conta.ConsultarStatusAsync(ct);
        return Ok(new { usuario.AsaasAccountId, usuario.StatusContaAsaas });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
    }
  }
}
