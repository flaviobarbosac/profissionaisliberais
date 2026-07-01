using Asp.Versioning;
using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Infraestructure.Services;
using Libify.Services.Asaas;
using Libify.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
  public class CobrancaController : BaseController<Cobranca, CobrancaDto>
  {
    private readonly ICobrancaAsaasService _asaas;

    public CobrancaController(
      IBaseServices<Cobranca> services,
      IMapper mapper,
      ICobrancaAsaasService asaas) : base(services, mapper)
    {
      _asaas = asaas;
    }

  /// <summary>Emite cobrança no Asaas e persiste localmente.</summary>
    [HttpPost("emitir")]
    public async Task<ActionResult<CobrancaDto>> Emitir([FromBody] EmitirCobrancaDto dados, CancellationToken ct)
    {
      try
      {
        var cobranca = await _asaas.EmitirAsync(new EmitirCobrancaRequest(
          dados.ClienteId,
          dados.Valor,
          dados.Vencimento,
          dados.FormaPagamento,
          dados.PropostaId,
          dados.Parcelas,
          dados.Descricao), ct);

        return Ok(Mapper.Map<CobrancaDto>(cobranca));
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
      catch (AsaasApiException ex)
      {
        return StatusCode((int)ex.StatusCode, new { erro = ex.Message, detalhes = ex.Errors });
      }
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
      try
      {
        await _asaas.CancelarAsync(id, ct);
        return NoContent();
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

    public override Task<ActionResult<CobrancaDto>> Post([FromBody] CobrancaDto dados)
      => Task.FromResult<ActionResult<CobrancaDto>>(BadRequest(new
      {
        erro = "Use POST /api/v1/cobranca/emitir para criar cobranças via Asaas."
      }));
  }
}
