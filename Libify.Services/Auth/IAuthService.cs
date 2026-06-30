namespace Libify.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> LoginGoogleAsync(string idToken, DispositivoInfo dispositivo, CancellationToken cancellationToken = default);
        Task<AuthResult> RefreshAsync(string refreshToken, DispositivoInfo dispositivo, CancellationToken cancellationToken = default);
    }
}
