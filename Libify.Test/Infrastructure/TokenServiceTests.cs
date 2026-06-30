using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Libify.Domain.Model;
using Libify.Infraestructure.Security;
using Microsoft.Extensions.Options;

namespace Libify.Test.Infrastructure
{
    public class TokenServiceTests
    {
        private static TokenService Criar() => new(Options.Create(new JwtOptions
        {
            Key = new string('k', 40),
            Issuer = "Libify",
            Audience = "LibifyApp",
            AccessTokenMinutes = 30,
            RefreshTokenDays = 15
        }));

        [Fact]
        public void GerarAccessToken_IncluiClaimsDeTelefone()
        {
            var service = Criar();
            var usuario = new Usuario { Id = Guid.CreateVersion7(), Nome = "Ana", Telefone = "11999990000" };
            var dispositivoId = Guid.CreateVersion7();

            var (token, expiraEm) = service.GerarAccessToken(usuario, dispositivoId);

            expiraEm.Should().BeAfter(DateTime.UtcNow);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == usuario.Id.ToString());
            jwt.Claims.Should().Contain(c => c.Type == "sid" && c.Value == dispositivoId.ToString());
            jwt.Claims.Should().Contain(c => c.Type == "phone" && c.Value == "11999990000");
            jwt.Issuer.Should().Be("Libify");
        }

        [Fact]
        public void GerarAccessToken_IncluiClaimDeDocumento()
        {
            var service = Criar();
            var usuario = new Usuario { Id = Guid.CreateVersion7(), Nome = "Empresa", CpfCnpj = "12345678000199" };

            var (token, _) = service.GerarAccessToken(usuario, Guid.CreateVersion7());

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            jwt.Claims.Should().Contain(c => c.Type == "doc" && c.Value == "12345678000199");
            jwt.Claims.Should().NotContain(c => c.Type == "phone");
        }

        [Fact]
        public void GerarRefreshToken_GeraValoresUnicos()
        {
            var service = Criar();

            var a = service.GerarRefreshToken();
            var b = service.GerarRefreshToken();

            a.Should().NotBeNullOrEmpty();
            a.Should().NotBe(b);
        }

        [Fact]
        public void Hash_EhDeterministico()
        {
            var service = Criar();

            service.Hash("123456").Should().Be(service.Hash("123456"));
            service.Hash("123456").Should().NotBe(service.Hash("654321"));
        }
    }
}
