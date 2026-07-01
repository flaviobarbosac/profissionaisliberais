using FluentAssertions;
using Libify.Domain.Ports;
using Libify.Infraestructure.Security;
using Libify.Infraestructure.Services;

namespace Libify.Test.Infrastructure
{
  public class AesSecretProtectorTests
  {
    [Fact]
    public void Protect_Unprotect_RoundTrip()
    {
      var protector = new AesSecretProtector(new AsaasConfig { EncryptionKey = "chave-teste-com-32-caracteres!!" });
      var plain = "asaas_api_key_secreta";

      var protectedText = protector.Protect(plain);
      protector.Unprotect(protectedText).Should().Be(plain);
    }
  }
}
