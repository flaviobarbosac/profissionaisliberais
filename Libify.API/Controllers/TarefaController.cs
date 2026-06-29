using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class TarefaController : BaseController<Tarefa, TarefaDto>
    {
        public TarefaController(IBaseServices<Tarefa> services, IMapper mapper) : base(services, mapper) { }
    }
}
