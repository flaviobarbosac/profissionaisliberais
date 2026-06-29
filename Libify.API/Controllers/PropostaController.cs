using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class PropostaController : BaseController<Proposta, PropostaDto>
    {
        public PropostaController(IBaseServices<Proposta> services, IMapper mapper) : base(services, mapper) { }
    }
}
