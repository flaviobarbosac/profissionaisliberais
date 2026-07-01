using System.Security.Cryptography;
using Libify.Domain.Enum;
using Libify.Domain.Helpers;
using Libify.Domain.Messaging;
using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Libify.Services.Proposta
{
  public class PropostaPortalService : IPropostaPortalService
  {
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IMessageBus _bus;
    private readonly PortalConfig _portal;
    private readonly IPropostaFechamentoService _fechamento;

    public PropostaPortalService(
      AppDbContext db,
      ITenantContext tenant,
      IMessageBus bus,
      PortalConfig portal,
      IPropostaFechamentoService fechamento)
    {
      _db = db;
      _tenant = tenant;
      _bus = bus;
      _portal = portal;
      _fechamento = fechamento;
    }

    public async Task<PropostaLinkResult> EnviarAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
      var proposta = await ObterPropostaTenantAsync(propostaId, cancellationToken);

      if (proposta.Status is PropostaStatus.Aceita or PropostaStatus.Recusada)
        throw new InvalidOperationException("Proposta já respondida pelo cliente.");

      GarantirToken(proposta);
      proposta.LinkExpiraEm = DateTimeHelper.UtcNow.AddDays(Math.Max(_portal.LinkValidadeDias, 1));
      proposta.Status = PropostaStatus.Enviada;
      proposta.EnviadaEm ??= DateTimeHelper.UtcNow;

      await PersistirAsync(proposta, cancellationToken);
      return MontarLink(proposta);
    }

    public async Task<PropostaLinkResult> ObterOuGerarLinkAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
      var proposta = await ObterPropostaTenantAsync(propostaId, cancellationToken);

      if (string.IsNullOrWhiteSpace(proposta.TokenPublico))
      {
        GarantirToken(proposta);
        proposta.LinkExpiraEm = DateTimeHelper.UtcNow.AddDays(Math.Max(_portal.LinkValidadeDias, 1));
        await PersistirAsync(proposta, cancellationToken);
      }

      return MontarLink(proposta);
    }

    public async Task<PropostaPortalView> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default)
    {
      var proposta = await ObterPropostaPorTokenAsync(token, cancellationToken)
        ?? throw new InvalidOperationException("Proposta não encontrada.");

      return MapearVisao(proposta);
    }

    public async Task<PropostaPortalView> AceitarAsync(
      string token,
      string? respondidoPor,
      string? ipAceite = null,
      CancellationToken cancellationToken = default)
    {
      var proposta = await ObterPropostaPorTokenAsync(token, cancellationToken)
        ?? throw new InvalidOperationException("Proposta não encontrada.");

      ValidarPodeResponder(proposta);

      var agora = DateTimeHelper.UtcNow;
      proposta.Status = PropostaStatus.Aceita;
      proposta.RespondidaEm = agora;
      proposta.RespondidoPor = NormalizarNome(respondidoPor);

      await PersistirAsync(proposta, cancellationToken);

      var fechamento = await _fechamento.ConcluirAceiteAsync(
        proposta,
        proposta.RespondidoPor,
        ipAceite,
        cancellationToken);

      return MapearVisao(proposta, fechamento);
    }

    public async Task<PropostaPortalView> RecusarAsync(
      string token,
      string? respondidoPor,
      string? motivo,
      CancellationToken cancellationToken = default)
    {
      var proposta = await ObterPropostaPorTokenAsync(token, cancellationToken)
        ?? throw new InvalidOperationException("Proposta não encontrada.");

      ValidarPodeResponder(proposta);

      var agora = DateTimeHelper.UtcNow;
      proposta.Status = PropostaStatus.Recusada;
      proposta.RespondidaEm = agora;
      proposta.RespondidoPor = NormalizarNome(respondidoPor);
      proposta.MotivoRecusa = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

      await PersistirAsync(proposta, cancellationToken);
      return MapearVisao(proposta);
    }

    private async Task<Domain.Model.Proposta> ObterPropostaTenantAsync(Guid propostaId, CancellationToken cancellationToken)
    {
      if (!_tenant.UsuarioId.HasValue)
        throw new UnauthorizedAccessException("Usuário não autenticado.");

      return await _db.Proposta
        .Include(p => p.Itens)
        .FirstOrDefaultAsync(p => p.Id == propostaId, cancellationToken)
        ?? throw new InvalidOperationException("Proposta não encontrada.");
    }

    private async Task<Domain.Model.Proposta?> ObterPropostaPorTokenAsync(string token, CancellationToken cancellationToken)
    {
      if (string.IsNullOrWhiteSpace(token))
        return null;

      var proposta = await _db.Proposta
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.TokenPublico == token && p.DeletedAt == null, cancellationToken);

      if (proposta is null)
        return null;

      proposta.Itens = await _db.PropostaItem
        .IgnoreQueryFilters()
        .Where(i => i.PropostaId == proposta.Id && i.DeletedAt == null)
        .OrderBy(i => i.CreatedAt)
        .ToListAsync(cancellationToken);

      proposta.Cliente = await _db.Cliente
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(c => c.Id == proposta.ClienteId && c.DeletedAt == null, cancellationToken);

      proposta.Usuario = await _db.Usuario
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Id == proposta.UsuarioId && u.DeletedAt == null, cancellationToken);

      return proposta;
    }

    private static void GarantirToken(Domain.Model.Proposta proposta)
    {
      if (string.IsNullOrWhiteSpace(proposta.TokenPublico))
        proposta.TokenPublico = GerarToken();
    }

    private static string GerarToken()
    {
      Span<byte> bytes = stackalloc byte[32];
      RandomNumberGenerator.Fill(bytes);
      return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static void ValidarPodeResponder(Domain.Model.Proposta proposta)
    {
      if (proposta.Status is PropostaStatus.Aceita or PropostaStatus.Recusada)
        throw new InvalidOperationException("Proposta já respondida.");

      if (proposta.Status != PropostaStatus.Enviada)
        throw new InvalidOperationException("Proposta ainda não foi enviada ao cliente.");

      if (proposta.LinkExpiraEm.HasValue && proposta.LinkExpiraEm.Value < DateTimeHelper.UtcNow)
        throw new InvalidOperationException("Link expirado.");
    }

    private static bool PodeResponder(Domain.Model.Proposta proposta)
    {
      if (proposta.Status != PropostaStatus.Enviada)
        return false;

      if (proposta.LinkExpiraEm.HasValue && proposta.LinkExpiraEm.Value < DateTimeHelper.UtcNow)
        return false;

      return true;
    }

    private PropostaLinkResult MontarLink(Domain.Model.Proposta proposta)
    {
      var baseUrl = (_portal.FrontBaseUrl ?? string.Empty).TrimEnd('/');
      var path = $"/portal/proposta/{proposta.TokenPublico}";
      var url = string.IsNullOrEmpty(baseUrl) ? path : $"{baseUrl}{path}";
      return new PropostaLinkResult(proposta.TokenPublico!, url, proposta.LinkExpiraEm);
    }

    private static PropostaPortalView MapearVisao(
      Domain.Model.Proposta proposta,
      FechamentoAceiteResult? fechamento = null)
    {
      var itens = proposta.Itens
        .OrderBy(i => i.CreatedAt)
        .Select(i => new PropostaPortalItemView(
          i.Descricao,
          i.Quantidade,
          i.ValorUnitario,
          i.Total))
        .ToList();

      PropostaPortalFechamentoView? fechamentoView = fechamento is null
        ? null
        : new PropostaPortalFechamentoView(
          fechamento.ContratoId,
          fechamento.CobrancaId,
          fechamento.LinkPagamento,
          fechamento.PixCopiaECola,
          fechamento.CobrancaAsaasEmitida,
          fechamento.AvisoAsaas);

      return new PropostaPortalView(
        proposta.Id,
        proposta.Usuario?.Nome ?? "Profissional",
        proposta.Cliente?.Nome ?? "Cliente",
        proposta.Titulo,
        (int)proposta.Status,
        proposta.ValorTotal,
        proposta.Observacoes,
        proposta.EnviadaEm,
        proposta.LinkExpiraEm,
        PodeResponder(proposta),
        itens,
        fechamentoView);
    }

    private static string? NormalizarNome(string? nome)
      => string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();

    private async Task PersistirAsync(Domain.Model.Proposta proposta, CancellationToken cancellationToken)
    {
      proposta.UpdatedAt = DateTimeHelper.UtcNow;
      proposta.Version += 1;

      await _bus.PublishAsync(new SyncEvent<Domain.Model.Proposta>
      {
        EntityId = proposta.Id,
        UsuarioId = proposta.UsuarioId,
        Version = proposta.Version,
        Operacao = SyncOperacao.Upsert,
        Payload = proposta,
        OcorridoEm = DateTimeHelper.UtcNow
      }, cancellationToken);

      await _db.SaveChangesAsync(cancellationToken);
    }
  }
}
