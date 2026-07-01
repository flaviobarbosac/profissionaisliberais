using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;
using Libify.Services.Proposta;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
  public class PropostaController : BaseController<Proposta, PropostaDto>
  {
    private readonly IPropostaPortalService _portal;
    private readonly IPropostaAppService _propostas;

    public PropostaController(
      IBaseServices<Proposta> services,
      IMapper mapper,
      IPropostaPortalService portal,
      IPropostaAppService propostas) : base(services, mapper)
    {
      _portal = portal;
      _propostas = propostas;
    }

    public override async Task<IActionResult> Get([FromQuery] int? page, [FromQuery] int? pageSize)
    {
      if (page is null)
      {
        var todos = await _propostas.GetPagedAsync(1, int.MaxValue);
        return Ok(Mapper.Map<IEnumerable<PropostaDto>>(todos.Items));
      }

      var tamanho = Math.Clamp(pageSize ?? 20, 1, 100);
      var (items, total) = await _propostas.GetPagedAsync(Math.Max(page.Value, 1), tamanho);
      Response.Headers["X-Total-Count"] = total.ToString();
      return Ok(Mapper.Map<IEnumerable<PropostaDto>>(items));
    }

    public override async Task<ActionResult<PropostaDto>> Get(Guid id)
    {
      var resultado = await _propostas.GetByIdAsync(id);
      if (resultado is null)
        return NotFound();

      return Ok(Mapper.Map<PropostaDto>(resultado));
    }

    public override async Task<ActionResult<PropostaDto>> Post([FromBody] PropostaDto dados)
    {
      var proposta = Mapper.Map<Proposta>(dados);
      var itens = Mapper.Map<IEnumerable<PropostaItem>>(dados.Itens);
      var criada = await _propostas.CreateAsync(proposta, itens);
      return CreatedAtAction(nameof(Get), new { id = criada.Id }, Mapper.Map<PropostaDto>(criada));
    }

    public override async Task<IActionResult> Put(Guid id, [FromBody] PropostaDto dados)
    {
      var proposta = Mapper.Map<Proposta>(dados);
      proposta.Id = id;
      var itens = Mapper.Map<IEnumerable<PropostaItem>>(dados.Itens);
      var atualizada = await _propostas.UpdateAsync(proposta, itens);
      return Ok(Mapper.Map<PropostaDto>(atualizada));
    }

    /// <summary>Marca como enviada, gera token e retorna link público.</summary>
    [HttpPost("{id:guid}/enviar")]
    public async Task<ActionResult<PropostaLinkDto>> Enviar(Guid id, CancellationToken ct)
    {
      try
      {
        var link = await _portal.EnviarAsync(id, ct);
        return Ok(MapLink(link));
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { erro = ex.Message });
      }
      catch (UnauthorizedAccessException ex)
      {
        return Unauthorized(new { erro = ex.Message });
      }
    }

    /// <summary>Retorna link público existente ou gera token sem alterar status.</summary>
    [HttpGet("{id:guid}/link-publico")]
    public async Task<ActionResult<PropostaLinkDto>> LinkPublico(Guid id, CancellationToken ct)
    {
      try
      {
        var link = await _portal.ObterOuGerarLinkAsync(id, ct);
        return Ok(MapLink(link));
      }
      catch (InvalidOperationException ex)
      {
        return NotFound(new { erro = ex.Message });
      }
      catch (UnauthorizedAccessException ex)
      {
        return Unauthorized(new { erro = ex.Message });
      }
    }

    private static PropostaLinkDto MapLink(PropostaLinkResult link) => new()
    {
      Token = link.Token,
      Url = link.Url,
      ExpiraEm = link.ExpiraEm
    };
  }
}
