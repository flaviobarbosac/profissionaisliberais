using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class LancamentoFinanceiroController : BaseController<LancamentoFinanceiro, LancamentoFinanceiroDto>
    {
        public LancamentoFinanceiroController(IBaseServices<LancamentoFinanceiro> services, IMapper mapper) : base(services, mapper) { }
    }
}
