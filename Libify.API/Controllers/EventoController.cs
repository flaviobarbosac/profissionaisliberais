using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class EventoController : BaseController<Evento, EventoDto>
    {
        public EventoController(IBaseServices<Evento> services, IMapper mapper) : base(services, mapper) { }
    }
}
