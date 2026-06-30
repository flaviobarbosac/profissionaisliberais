namespace Libify.Domain.Ports
{
    /// <summary>Dados de identidade extraídos de um id_token válido do Google.</summary>
    public record GoogleUserInfo(
        string Subject,
        string Email,
        bool EmailVerificado,
        string? Nome,
        string? FotoUrl);

    /// <summary>
    /// Validação do id_token do Google (login social). Não acessa o Drive:
    /// o upload de arquivos é feito client-side pelo app com o access token do usuário.
    /// </summary>
    public interface IGoogleAuthClient
    {
        Task<GoogleUserInfo> ValidarIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
    }
}
