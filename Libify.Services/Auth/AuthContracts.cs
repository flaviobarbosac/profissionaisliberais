namespace Libify.Services.Auth
{
    public record DispositivoInfo(
        string DeviceId,
        string? Nome = null,
        string? Plataforma = null,
        string? AppVersion = null,
        string? Ip = null,
        string? UserAgent = null);

    public record AuthResult(
        Guid UsuarioId,
        string Nome,
        Guid DispositivoId,
        string AccessToken,
        DateTime ExpiraEm,
        string RefreshToken,
        DateTime RefreshExpiraEm);
}
