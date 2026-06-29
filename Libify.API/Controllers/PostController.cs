using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Services.Interface;

namespace Libify.API.Controllers
{
    public class PostController : BaseController<Post, PostDto>
    {
        public PostController(IBaseServices<Post> services, IMapper mapper) : base(services, mapper) { }
    }
}
