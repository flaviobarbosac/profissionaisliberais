using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class ClienteController : BaseController<Cliente, ClienteDto>
    {
        public ClienteController(IBaseServices<Cliente> services, IMapper mapper) : base(services, mapper) { }
    }
}
