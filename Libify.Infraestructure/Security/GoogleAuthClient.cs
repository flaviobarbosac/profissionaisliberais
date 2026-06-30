using Google.Apis.Auth;
using Libify.Domain.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libify.Infraestructure.Security
{
    public class GoogleAuthOptions
    {
        /// <summary>OAuth Client ID do Google; usado como audience na validação do id_token.</summary>
        public string ClientId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Valida o id_token do Google (assinatura, expiração e audience = Google:ClientId)
    /// usando a biblioteca oficial Google.Apis.Auth.
    /// </summary>
    public class GoogleAuthClient : IGoogleAuthClient
    {
        private readonly GoogleAuthOptions _options;
        private readonly ILogger<GoogleAuthClient> _logger;

        public GoogleAuthClient(IOptions<GoogleAuthOptions> options, ILogger<GoogleAuthClient> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<GoogleUserInfo> ValidarIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId))
                throw new InvalidOperationException("Google:ClientId não configurado.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _options.ClientId },
                    IssuedAtClockTolerance = TimeSpan.FromMinutes(5),
                    ExpirationTimeClockTolerance = TimeSpan.FromMinutes(5)
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Falha ao validar id_token do Google: {Motivo}", ex.Message);
                throw new InvalidOperationException("Token Google inválido.");
            }

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.Name,
                payload.Picture);
        }
    }
}
