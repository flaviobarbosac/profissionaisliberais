using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Libify.Domain.Model;
using Libify.Domain.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Libify.Infraestructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public (string accessToken, DateTime expiraEm) GerarAccessToken(Usuario usuario, Guid dispositivoId)
        {
            var expiraEm = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new("sub", usuario.Id.ToString()),
                new("sid", dispositivoId.ToString()),
                new("name", usuario.Nome)
            };
            if (!string.IsNullOrWhiteSpace(usuario.Telefone))
                claims.Add(new Claim("phone", usuario.Telefone));
            if (!string.IsNullOrWhiteSpace(usuario.CpfCnpj))
                claims.Add(new Claim("doc", usuario.CpfCnpj));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiraEm,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
        }

        public string GerarRefreshToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public string Hash(string valor)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
    }
}
