using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    /// <summary>
    /// Somente leitura: a criação/atualização/remoção do Usuario (raiz do tenant)
    /// é controlada pelo fluxo de autenticação, não pelo CRUD genérico.
    /// </summary>
    public class UsuarioController : ReadOnlyController<Usuario, UsuarioDto>
    {
        public UsuarioController(IBaseServices<Usuario> services, IMapper mapper) : base(services, mapper) { }
    }
}
