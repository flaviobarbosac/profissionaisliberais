using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    /// <summary>
    /// Somente leitura: a assinatura (Plano) é criada/atualizada pelo fluxo de
    /// assinatura recorrente (Asaas), não pelo CRUD genérico — evita burlar pagamento.
    /// </summary>
    public class PlanoController : ReadOnlyController<Plano, PlanoDto>
    {
        public PlanoController(IBaseServices<Plano> services, IMapper mapper) : base(services, mapper) { }
    }
}
