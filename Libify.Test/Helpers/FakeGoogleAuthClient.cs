using Libify.Domain.Ports;

namespace Libify.Test.Helpers
{
    /// <summary>Fake de validação Google: devolve um perfil configurável sem chamar a rede.</summary>
    public class FakeGoogleAuthClient : IGoogleAuthClient
    {
        public GoogleUserInfo Proximo { get; set; } =
            new("google-sub-1", "user@gmail.com", true, "Usuário Teste", null);

        public Task<GoogleUserInfo> ValidarIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
            => Task.FromResult(Proximo);
    }
}
