using FluentAssertions;
using Libify.Domain.Enum;
using Libify.Domain.Model;
using Libify.Infraestructure.Services;
using Libify.Services.Asaas;
using Libify.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Libify.Test.Services
{
  public class AsaasWebhookProcessorTests
  {
    [Fact]
    public async Task Processar_PaymentReceived_AtualizaCobrancaECriaLancamento()
    {
      var tenant = Guid.CreateVersion7();
      var tc = new TestTenantContext(tenant);
      using var db = TestDbContextHelper.CreateInMemory(tc);
      var config = new AsaasConfig { WebhookAuthToken = "tok" };

      var cobrancaId = Guid.CreateVersion7();
      db.Cobranca.Add(new Cobranca
      {
        Id = cobrancaId,
        UsuarioId = tenant,
        ClienteId = Guid.CreateVersion7(),
        AsaasPaymentId = "pay_1",
        Valor = 100,
        Vencimento = DateTime.UtcNow.AddDays(3),
        Status = StatusCobranca.Pendente,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Version = 1
      });
      await db.SaveChangesAsync();

      var processor = new AsaasWebhookProcessor(db, config);
      var payload = """
        {"event":"PAYMENT_RECEIVED","payment":{"id":"pay_1","status":"RECEIVED"}}
        """;

      var resultado = await processor.ProcessarAsync(payload, "tok");

      resultado.Should().Be("ok");
      var cobranca = db.Cobranca.IgnoreQueryFilters().First(c => c.Id == cobrancaId);
      cobranca.Status.Should().Be(StatusCobranca.Recebida);
      db.LancamentoFinanceiro.IgnoreQueryFilters().Should().ContainSingle();
    }

    [Fact]
    public async Task Processar_TokenInvalido_LancaUnauthorized()
    {
      using var db = TestDbContextHelper.CreateInMemory(new TestTenantContext(Guid.CreateVersion7()));
      var processor = new AsaasWebhookProcessor(db, new AsaasConfig { WebhookAuthToken = "tok" });

      var act = async () => await processor.ProcessarAsync("{}", "errado");

      await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Processar_EventoDuplicado_RetornaIgnorado()
    {
      using var db = TestDbContextHelper.CreateInMemory(new TestTenantContext(Guid.CreateVersion7()));
      var config = new AsaasConfig { WebhookAuthToken = "tok" };
      var processor = new AsaasWebhookProcessor(db, config);
      var payload = """{"event":"PAYMENT_CREATED","payment":{"id":"pay_x","status":"PENDING"}}""";

      await processor.ProcessarAsync(payload, "tok");
      var segundo = await processor.ProcessarAsync(payload, "tok");

      segundo.Should().Be("ignorado");
    }
  }
}
