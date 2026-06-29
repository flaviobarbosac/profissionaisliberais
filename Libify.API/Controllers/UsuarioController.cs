using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class UsuarioController : BaseController<Usuario, UsuarioDto>
    {
        public UsuarioController(IBaseServices<Usuario> services, IMapper mapper) : base(services, mapper) { }
    }
}
