using FluentAssertions;
using Libify.Domain.Enum;
using Libify.Domain.Model;
using Libify.Infraestructure.Services;
using Libify.Services.Proposta;
using Libify.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Libify.Test.Services
{
  public class PropostaPortalServiceTests
  {
    [Fact]
    public async Task Enviar_GeraTokenEAlteraStatusParaEnviada()
    {
      var tenantId = Guid.CreateVersion7();
      var tc = new TestTenantContext(tenantId);
      using var db = TestDbContextHelper.CreateInMemory(tc);
      var proposta = await SeedPropostaAsync(db, tenantId, PropostaStatus.Rascunho);

      var service = new PropostaPortalService(db, tc, new NullMessageBus(), new PortalConfig
      {
        FrontBaseUrl = "https://app.test",
        LinkValidadeDias = 7
      });

      var link = await service.EnviarAsync(proposta.Id);

      link.Token.Should().NotBeNullOrEmpty();
      link.Url.Should().Contain(link.Token);
      link.ExpiraEm.Should().NotBeNull();

      var atualizada = await db.Proposta.FindAsync(proposta.Id);
      atualizada!.Status.Should().Be(PropostaStatus.Enviada);
      atualizada.TokenPublico.Should().Be(link.Token);
      atualizada.EnviadaEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Aceitar_PorToken_AlteraStatusParaAceita()
    {
      var tenantId = Guid.CreateVersion7();
      var tc = new TestTenantContext(tenantId);
      using var db = TestDbContextHelper.CreateInMemory(tc);
      var proposta = await SeedPropostaAsync(db, tenantId, PropostaStatus.Enviada, token: "abc123", expiraEm: DateTime.UtcNow.AddDays(1));

      var service = new PropostaPortalService(db, tc, new NullMessageBus(), new PortalConfig());

      var view = await service.AceitarAsync("abc123", "Maria Cliente");

      view.Status.Should().Be((int)PropostaStatus.Aceita);
      view.PodeResponder.Should().BeFalse();

      var atualizada = await db.Proposta.IgnoreQueryFilters().FirstAsync(p => p.Id == proposta.Id);
      atualizada.Status.Should().Be(PropostaStatus.Aceita);
      atualizada.RespondidoPor.Should().Be("Maria Cliente");
      atualizada.RespondidaEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Recusar_LinkExpirado_LancaInvalidOperation()
    {
      var tenantId = Guid.CreateVersion7();
      var tc = new TestTenantContext(tenantId);
      using var db = TestDbContextHelper.CreateInMemory(tc);
      await SeedPropostaAsync(db, tenantId, PropostaStatus.Enviada, token: "expired", expiraEm: DateTime.UtcNow.AddDays(-1));

      var service = new PropostaPortalService(db, tc, new NullMessageBus(), new PortalConfig());

      var act = () => service.RecusarAsync("expired", null, "Caro demais");
      await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*expirado*");
    }

    private static async Task<Proposta> SeedPropostaAsync(
      Libify.Infraestructure.Database.AppDbContext db,
      Guid tenantId,
      PropostaStatus status,
      string? token = null,
      DateTime? expiraEm = null)
    {
      var usuario = new Usuario
      {
        Id = tenantId,
        Nome = "Prestador",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Version = 1
      };
      var cliente = new Cliente
      {
        Id = Guid.CreateVersion7(),
        UsuarioId = tenantId,
        Nome = "Cliente Teste",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Version = 1
      };
      var proposta = new Proposta
      {
        Id = Guid.CreateVersion7(),
        UsuarioId = tenantId,
        ClienteId = cliente.Id,
        Titulo = "Projeto X",
        Status = status,
        ValorTotal = 100m,
        TokenPublico = token,
        LinkExpiraEm = expiraEm,
        EnviadaEm = status == PropostaStatus.Enviada ? DateTime.UtcNow : null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Version = 1,
        Itens =
        [
          new PropostaItem
          {
            Id = Guid.CreateVersion7(),
            UsuarioId = tenantId,
            Descricao = "Servico A",
            Quantidade = 1,
            ValorUnitario = 100m,
            Total = 100m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1
          }
        ]
      };

      db.Usuario.Add(usuario);
      db.Cliente.Add(cliente);
      db.Proposta.Add(proposta);
      foreach (var item in proposta.Itens)
        item.PropostaId = proposta.Id;
      await db.SaveChangesAsync();
      return proposta;
    }
  }
}
