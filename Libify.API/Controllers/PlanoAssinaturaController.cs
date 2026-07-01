using Asp.Versioning;
using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Infraestructure.Services;
using Libify.Services.Asaas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/plano-assinatura")]
  [Authorize]
  public class PlanoAssinaturaController : ControllerBase
  {
    private readonly IPlanoAssinaturaService _assinatura;
    private readonly IMapper _mapper;

    public PlanoAssinaturaController(IPlanoAssinaturaService assinatura, IMapper mapper)
    {
      _assinatura = assinatura;
      _mapper = mapper;
    }

    [HttpPost("assinar")]
    public async Task<ActionResult<PlanoDto>> Assinar([FromBody] AssinarPlanoDto dados, CancellationToken ct)
    {
      try
      {
        var plano = await _assinatura.AssinarAsync(
          new AssinarPlanoRequest(dados.Tipo, dados.FormaPagamento), ct);
        return Ok(_mapper.Map<PlanoDto>(plano));
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
      catch (AsaasApiException ex)
      {
        return StatusCode((int)ex.StatusCode, new { erro = ex.Message });
      }
    }

    [HttpDelete]
    public async Task<IActionResult> Cancelar(CancellationToken ct)
    {
      try
      {
        await _assinatura.CancelarAsync(ct);
        return NoContent();
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
    }
  }
}
