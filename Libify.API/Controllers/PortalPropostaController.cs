using Asp.Versioning;
using Libify.API.Dto;
using Libify.Services.Proposta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/portal/proposta")]
  [AllowAnonymous]
  public class PortalPropostaController : ControllerBase
  {
    private readonly IPropostaPortalService _portal;

    public PortalPropostaController(IPropostaPortalService portal) => _portal = portal;

    [HttpGet("{token}")]
    public async Task<ActionResult<PropostaPortalDto>> Obter(string token, CancellationToken ct)
    {
      try
      {
        var view = await _portal.ObterPorTokenAsync(token, ct);
        return Ok(Map(view));
      }
      catch (InvalidOperationException ex)
      {
        return NotFound(new { erro = ex.Message });
      }
    }

    [HttpPost("{token}/aceitar")]
    public async Task<ActionResult<PropostaPortalDto>> Aceitar(
      string token,
      [FromBody] ResponderPropostaPortalDto? dados,
      CancellationToken ct)
    {
      try
      {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var view = await _portal.AceitarAsync(token, dados?.Nome, ip, ct);
        return Ok(Map(view));
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
    }

    [HttpPost("{token}/recusar")]
    public async Task<ActionResult<PropostaPortalDto>> Recusar(
      string token,
      [FromBody] RecusarPropostaPortalDto? dados,
      CancellationToken ct)
    {
      try
      {
        var view = await _portal.RecusarAsync(token, dados?.Nome, dados?.Motivo, ct);
        return Ok(Map(view));
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
    }

    private static PropostaPortalDto Map(PropostaPortalView view) => new()
    {
      Id = view.Id,
      ProfissionalNome = view.ProfissionalNome,
      ClienteNome = view.ClienteNome,
      Titulo = view.Titulo,
      Status = view.Status,
      ValorTotal = view.ValorTotal,
      Observacoes = view.Observacoes,
      EnviadaEm = view.EnviadaEm,
      LinkExpiraEm = view.LinkExpiraEm,
      PodeResponder = view.PodeResponder,
      Itens = view.Itens.Select(i => new PropostaPortalItemDto
      {
        Descricao = i.Descricao,
        Quantidade = i.Quantidade,
        ValorUnitario = i.ValorUnitario,
        Total = i.Total
      }).ToList(),
      Fechamento = view.Fechamento is null ? null : new PropostaPortalFechamentoDto
      {
        ContratoId = view.Fechamento.ContratoId,
        CobrancaId = view.Fechamento.CobrancaId,
        LinkPagamento = view.Fechamento.LinkPagamento,
        PixCopiaECola = view.Fechamento.PixCopiaECola,
        CobrancaAsaasEmitida = view.Fechamento.CobrancaAsaasEmitida,
        AvisoAsaas = view.Fechamento.AvisoAsaas
      }
    };
  }
}
