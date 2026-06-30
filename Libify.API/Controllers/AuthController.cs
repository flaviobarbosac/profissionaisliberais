using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Libify.API.Dto;
using Libify.Services.Auth;

namespace Libify.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("google")]
        public async Task<ActionResult<AuthResponse>> LoginGoogle([FromBody] GoogleLoginRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.IdToken))
                return BadRequest(new { erro = "IdToken é obrigatório." });
            if (string.IsNullOrWhiteSpace(req.DeviceId))
                return BadRequest(new { erro = "DeviceId é obrigatório." });

            try
            {
                var info = MontarDispositivo(req.DeviceId, req.DeviceNome, req.Plataforma, req.AppVersion);
                var r = await _auth.LoginGoogleAsync(req.IdToken, info, ct);
                return Ok(Map(r));
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken) || string.IsNullOrWhiteSpace(req.DeviceId))
                return BadRequest(new { erro = "RefreshToken e DeviceId são obrigatórios." });

            try
            {
                var info = MontarDispositivo(req.DeviceId, req.DeviceNome, req.Plataforma, req.AppVersion);
                var r = await _auth.RefreshAsync(req.RefreshToken, info, ct);
                return Ok(Map(r));
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
        }

        private DispositivoInfo MontarDispositivo(string deviceId, string? nome, string? plataforma, string? appVersion)
            => new(
                deviceId,
                nome,
                plataforma,
                appVersion,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

        private static AuthResponse Map(AuthResult r) => new()
        {
            UsuarioId = r.UsuarioId,
            Nome = r.Nome,
            DispositivoId = r.DispositivoId,
            AccessToken = r.AccessToken,
            ExpiraEm = r.ExpiraEm,
            RefreshToken = r.RefreshToken,
            RefreshExpiraEm = r.RefreshExpiraEm
        };
    }
}
