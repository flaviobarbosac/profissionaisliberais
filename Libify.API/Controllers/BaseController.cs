using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Libify.Domain.Model.Base;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    /// <summary>
    /// Controller CRUD genérico. Cada recurso herda definindo a entidade e o DTO,
    /// evitando duplicação do mesmo fluxo Map/Service em todos os controllers.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<TEntity, TDto> : ControllerBase
        where TEntity : ModelBase
    {
        protected readonly IBaseServices<TEntity> Services;
        protected readonly IMapper Mapper;

        protected BaseController(IBaseServices<TEntity> services, IMapper mapper)
        {
            Services = services;
            Mapper = mapper;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            var resultado = await Services.GetAllAsync();
            return Ok(Mapper.Map<IEnumerable<TDto>>(resultado));
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TDto>> Get(int id)
        {
            var resultado = await Services.GetByIdAsync(id);
            if (resultado == null)
                return NotFound();

            return Ok(Mapper.Map<TDto>(resultado));
        }

        [HttpPost]
        public virtual async Task<ActionResult<TDto>> Post([FromBody] TDto dados)
        {
            var entidade = Mapper.Map<TEntity>(dados);
            await Services.AddAsync(entidade);
            var criado = Mapper.Map<TDto>(entidade);
            return CreatedAtAction(nameof(Get), new { id = entidade.Id }, criado);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Put(int id, [FromBody] TDto dados)
        {
            var entidade = Mapper.Map<TEntity>(dados);
            entidade.Id = id;
            await Services.UpdateAsync(entidade);
            return Ok(Mapper.Map<TDto>(entidade));
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            await Services.SoftDeleteAsync(id);
            return NoContent();
        }
    }
}
