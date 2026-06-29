using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class ServicoController : BaseController<Servico, ServicoDto>
    {
        public ServicoController(IBaseServices<Servico> services, IMapper mapper) : base(services, mapper) { }
    }
}
