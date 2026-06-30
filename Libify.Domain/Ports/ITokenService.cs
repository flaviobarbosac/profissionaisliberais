using Libify.Domain.Model;

namespace Libify.Domain.Ports
{
    /// <summary>
    /// Geração de tokens de acesso (JWT) e refresh. Claims: sub (UsuarioId) e sid (DispositivoId).
    /// </summary>
    public interface ITokenService
    {
        (string accessToken, DateTime expiraEm) GerarAccessToken(Usuario usuario, Guid dispositivoId);
        string GerarRefreshToken();
        string Hash(string valor);
    }
}
