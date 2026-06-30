using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Serilog;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Services.Auth;

namespace Libify.API.Middleware
{
    /// <summary>
    /// Resolve o tenant a partir das claims do JWT (sub = UsuarioId, sid = DispositivoId)
    /// e bloqueia requisições de dispositivos revogados. O status de revogação é cacheado
    /// (TTL curto) para evitar uma consulta ao banco em todo request autenticado.
    /// </summary>
    public class TenantMiddleware
    {
        private static readonly HybridCacheEntryOptions CacheOptions = new()
        {
            Expiration = TimeSpan.FromSeconds(30),
            LocalCacheExpiration = TimeSpan.FromSeconds(30)
        };
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITenantContext tenant, AppDbContext db, HybridCache cache, IDiagnosticContext diagnostic)
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated == true &&
                Guid.TryParse(user.FindFirst("sub")?.Value, out var usuarioId))
            {
                Guid? dispositivoId = Guid.TryParse(user.FindFirst("sid")?.Value, out var sid) ? sid : null;
                tenant.Set(usuarioId, dispositivoId);
                diagnostic.Set("TenantId", usuarioId);

                if (dispositivoId.HasValue)
                {
                    var revogado = await cache.GetOrCreateAsync(
                        DispositivoRevocationCache.Key(dispositivoId.Value),
                        async ct =>
                        {
                            var dispositivo = await db.Dispositivo.AsNoTracking()
                                .FirstOrDefaultAsync(d => d.Id == dispositivoId.Value, ct);
                            return dispositivo is null || dispositivo.Revogado;
                        },
                        CacheOptions,
                        cancellationToken: context.RequestAborted);

                    if (revogado)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { erro = "Dispositivo revogado ou inválido." });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
