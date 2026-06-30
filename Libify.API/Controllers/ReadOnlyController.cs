using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libify.Domain.Model.Base;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    /// <summary>
    /// Controller de leitura genérico e versionado (GET lista + GET por id).
    /// Exige autenticação em todos os ambientes. Recursos cuja escrita é controlada
    /// por fluxos próprios (ex.: Usuario via auth, Plano via assinatura) herdam só a leitura.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public abstract class ReadOnlyController<TEntity, TDto> : ControllerBase
        where TEntity : ModelBase
    {
        protected readonly IBaseServices<TEntity> Services;
        protected readonly IMapper Mapper;

        protected ReadOnlyController(IBaseServices<TEntity> services, IMapper mapper)
        {
            Services = services;
            Mapper = mapper;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            if (page is null)
            {
                var todos = await Services.GetAllAsync();
                return Ok(Mapper.Map<IEnumerable<TDto>>(todos));
            }

            var tamanho = Math.Clamp(pageSize ?? 20, 1, 100);
            var (items, total) = await Services.GetPagedAsync(Math.Max(page.Value, 1), tamanho);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(Mapper.Map<IEnumerable<TDto>>(items));
        }

        [HttpGet("{id:guid}")]
        public virtual async Task<ActionResult<TDto>> Get(Guid id)
        {
            var resultado = await Services.GetByIdAsync(id);
            if (resultado == null)
                return NotFound();

            return Ok(Mapper.Map<TDto>(resultado));
        }
    }
}
