using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class NotaFiscalController : BaseController<NotaFiscal, NotaFiscalDto>
    {
        public NotaFiscalController(IBaseServices<NotaFiscal> services, IMapper mapper) : base(services, mapper) { }
    }
}
