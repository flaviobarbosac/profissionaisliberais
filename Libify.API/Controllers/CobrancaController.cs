using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class CobrancaController : BaseController<Cobranca, CobrancaDto>
    {
        public CobrancaController(IBaseServices<Cobranca> services, IMapper mapper) : base(services, mapper) { }
    }
}
