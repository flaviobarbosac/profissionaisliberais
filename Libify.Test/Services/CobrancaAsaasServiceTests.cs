using FluentAssertions;
using Libify.Domain.Enum;
using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Infraestructure.Services;
using Libify.Infraestructure.Services.Interface;
using Libify.Repository;
using Libify.Services.Asaas;
using Libify.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Libify.Test.Services
{
  public class CobrancaAsaasServiceTests
  {
    [Fact]
    public async Task Emitir_CriaCobrancaNoAsaasEPersiste()
    {
      var tenantId = Guid.CreateVersion7();
      var tc = new TestTenantContext(tenantId);
      using var db = TestDbContextHelper.CreateInMemory(tc);

      var usuario = new Usuario
      {
        Id = tenantId,
        Nome = "Prestador",
        AsaasApiKey = "enc",
        StatusContaAsaas = StatusContaAsaas.Aprovada,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Version = 1
      };
      var cliente = new Cliente
      {
        Id = Guid.CreateVersion7(),
        UsuarioId = tenantId,
        Nome = "Cliente",
        CpfCnpj = "12345678901",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Version = 1
      };
      db.Usuario.Add(usuario);
      db.Cliente.Add(cliente);
      await db.SaveChangesAsync();

      var asaas = new Mock<IAsaasClient>();
      asaas.Setup(a => a.CriarClienteAsync(It.IsAny<string>(), It.IsAny<AsaasClienteRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AsaasClienteResponse("cus_1"));
      asaas.Setup(a => a.CriarCobrancaAsync(It.IsAny<string>(), It.IsAny<AsaasCobrancaRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AsaasCobrancaResponse("pay_1", "PENDING", 50m, "2026-07-01", null, null));
      asaas.Setup(a => a.ObterPixQrCodeAsync(It.IsAny<string>(), "pay_1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AsaasPixQrCodeResponse(null, "000201", null));

      var protector = new Mock<ISecretProtector>();
      protector.Setup(p => p.Unprotect("enc")).Returns("sub-key");

      var repo = new BaseRepository<Cobranca>(db, tc, new NullMessageBus());
      var service = new CobrancaAsaasService(db, tc, asaas.Object, protector.Object, repo);

      var cobranca = await service.EmitirAsync(new EmitirCobrancaRequest(
        cliente.Id, 50m, DateTime.UtcNow.AddDays(2), FormaPagamento.Pix));

      cobranca.AsaasPaymentId.Should().Be("pay_1");
      cobranca.PixCopiaECola.Should().Be("000201");
      var clienteDb = await db.Cliente.IgnoreQueryFilters().FirstAsync(c => c.Id == cliente.Id);
      clienteDb.AsaasCustomerId.Should().Be("cus_1");
    }
  }
}
