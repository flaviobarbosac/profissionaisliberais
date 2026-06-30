using Libify.Domain.Model;

namespace Libify.Services.Auth
{
    public interface IDispositivoService
    {
        Task<IEnumerable<Dispositivo>> ListarAsync(CancellationToken cancellationToken = default);
        Task<bool> RevogarAsync(Guid dispositivoId, CancellationToken cancellationToken = default);
        Task<int> RevogarOutrosAsync(CancellationToken cancellationToken = default);
    }
}
