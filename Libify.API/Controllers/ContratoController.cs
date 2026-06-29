using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class ContratoController : BaseController<Contrato, ContratoDto>
    {
        public ContratoController(IBaseServices<Contrato> services, IMapper mapper) : base(services, mapper) { }
    }
}
