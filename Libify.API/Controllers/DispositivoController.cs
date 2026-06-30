using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libify.API.Dto;
using Libify.Services.Auth;

namespace Libify.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/dispositivos")]
    [Authorize]
    public class DispositivoController : ControllerBase
    {
        private readonly IDispositivoService _dispositivos;
        private readonly IMapper _mapper;

        public DispositivoController(IDispositivoService dispositivos, IMapper mapper)
        {
            _dispositivos = dispositivos;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(CancellationToken ct)
        {
            var dispositivos = await _dispositivos.ListarAsync(ct);
            return Ok(_mapper.Map<IEnumerable<DispositivoDto>>(dispositivos));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Revogar(Guid id, CancellationToken ct)
        {
            var ok = await _dispositivos.RevogarAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("revogar-outros")]
        public async Task<IActionResult> RevogarOutros(CancellationToken ct)
        {
            var total = await _dispositivos.RevogarOutrosAsync(ct);
            return Ok(new { revogados = total });
        }
    }
}
