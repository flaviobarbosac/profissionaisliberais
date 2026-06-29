using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class PlanoController : BaseController<Plano, PlanoDto>
    {
        public PlanoController(IBaseServices<Plano> services, IMapper mapper) : base(services, mapper) { }
    }
}
