using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Libify.Domain.Model.Base;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    /// <summary>
    /// Controller CRUD genérico e versionado. Herda a leitura de <see cref="ReadOnlyController{TEntity, TDto}"/>
    /// e adiciona Post/Put/Delete para os recursos com ciclo de vida completo via API,
    /// evitando duplicação do mesmo fluxo Map/Service em todos os controllers.
    /// </summary>
    public abstract class BaseController<TEntity, TDto> : ReadOnlyController<TEntity, TDto>
        where TEntity : ModelBase
    {
        protected BaseController(IBaseServices<TEntity> services, IMapper mapper) : base(services, mapper) { }

        [HttpPost]
        public virtual async Task<ActionResult<TDto>> Post([FromBody] TDto dados)
        {
            var entidade = Mapper.Map<TEntity>(dados);
            await Services.AddAsync(entidade);
            var criado = Mapper.Map<TDto>(entidade);
            return CreatedAtAction(nameof(Get), new { id = entidade.Id }, criado);
        }

        [HttpPut("{id:guid}")]
        public virtual async Task<IActionResult> Put(Guid id, [FromBody] TDto dados)
        {
            var entidade = Mapper.Map<TEntity>(dados);
            entidade.Id = id;
            await Services.UpdateAsync(entidade);
            return Ok(Mapper.Map<TDto>(entidade));
        }

        [HttpDelete("{id:guid}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            await Services.SoftDeleteAsync(id);
            return NoContent();
        }
    }
}
