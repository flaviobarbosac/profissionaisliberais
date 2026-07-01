using Libify.Domain.Enum;
using Libify.Domain.Helpers;
using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Services;
using Libify.Infraestructure.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Libify.Services.Asaas
{
  public class ContaAsaasService : IContaAsaasService
  {
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IAsaasClient _asaas;
    private readonly ISecretProtector _protector;
    private readonly AsaasConfig _config;

    public ContaAsaasService(
      AppDbContext db,
      ITenantContext tenant,
      IAsaasClient asaas,
      ISecretProtector protector,
      AsaasConfig config)
    {
      _db = db;
      _tenant = tenant;
      _asaas = asaas;
      _protector = protector;
      _config = config;
    }

    public async Task<Usuario> CriarSubcontaAsync(CancellationToken cancellationToken = default)
    {
      var usuario = await ObterUsuarioAtualAsync(cancellationToken);
      if (!string.IsNullOrWhiteSpace(usuario.AsaasAccountId))
        throw new InvalidOperationException("Subconta Asaas já criada.");

      if (string.IsNullOrWhiteSpace(usuario.CpfCnpj))
        throw new InvalidOperationException("CPF/CNPJ é obrigatório para criar subconta Asaas.");

      var webhooks = MontarWebhooks();
      var request = new AsaasSubcontaRequest(
        Name: usuario.Nome,
        Email: usuario.Email ?? $"{usuario.Id:N}@libify.local",
        CpfCnpj: NormalizarDocumento(usuario.CpfCnpj),
        MobilePhone: usuario.Telefone,
        CompanyType: usuario.CpfCnpj.Length > 11 ? "MEI" : null,
        Webhooks: webhooks);

      var conta = await _asaas.CriarSubcontaAsync(request, cancellationToken);
      if (string.IsNullOrWhiteSpace(conta.Id) || string.IsNullOrWhiteSpace(conta.ApiKey))
        throw new InvalidOperationException("Asaas não retornou dados da subconta.");

      var agora = DateTimeHelper.UtcNow;
      usuario.AsaasAccountId = conta.Id;
      usuario.AsaasApiKey = _protector.Protect(conta.ApiKey);
      usuario.StatusContaAsaas = MapearStatusConta(conta.Status);
      usuario.UpdatedAt = agora;
      usuario.Version += 1;

      await _db.SaveChangesAsync(cancellationToken);
      return usuario;
    }

    public async Task<Usuario> ConsultarStatusAsync(CancellationToken cancellationToken = default)
    {
      var usuario = await ObterUsuarioAtualAsync(cancellationToken);
      if (string.IsNullOrWhiteSpace(usuario.AsaasAccountId) || string.IsNullOrWhiteSpace(usuario.AsaasApiKey))
        throw new InvalidOperationException("Subconta Asaas não configurada.");

      var apiKey = _protector.Unprotect(usuario.AsaasApiKey);
      var conta = await _asaas.ObterContaAsync(apiKey, usuario.AsaasAccountId, cancellationToken);

      usuario.StatusContaAsaas = MapearStatusConta(conta.Status);
      usuario.UpdatedAt = DateTimeHelper.UtcNow;
      usuario.Version += 1;
      await _db.SaveChangesAsync(cancellationToken);
      return usuario;
    }

    private async Task<Usuario> ObterUsuarioAtualAsync(CancellationToken cancellationToken)
    {
      if (!_tenant.UsuarioId.HasValue)
        throw new UnauthorizedAccessException("Usuário não autenticado.");

      var usuario = await _db.Usuario.FirstOrDefaultAsync(u => u.Id == _tenant.UsuarioId.Value, cancellationToken);
      return usuario ?? throw new InvalidOperationException("Usuário não encontrado.");
    }

    private AsaasWebhookConfig[]? MontarWebhooks()
    {
      if (string.IsNullOrWhiteSpace(_config.WebhookBaseUrl) || string.IsNullOrWhiteSpace(_config.WebhookAuthToken))
        return null;

      var url = $"{_config.WebhookBaseUrl.TrimEnd('/')}/api/v1/webhooks/asaas";
      return
      [
        new AsaasWebhookConfig(
          Name: "Libify Pagamentos",
          Url: url,
          Email: "webhooks@libify.local",
          AuthToken: _config.WebhookAuthToken,
          Events:
          [
            "PAYMENT_CREATED", "PAYMENT_UPDATED", "PAYMENT_CONFIRMED", "PAYMENT_RECEIVED",
            "PAYMENT_OVERDUE", "PAYMENT_DELETED", "PAYMENT_REFUNDED",
            "SUBSCRIPTION_CREATED", "SUBSCRIPTION_UPDATED", "SUBSCRIPTION_DELETED"
          ])
      ];
    }

    private static StatusContaAsaas MapearStatusConta(string? status)
      => status?.ToUpperInvariant() switch
      {
        "APPROVED" or "ACTIVE" => StatusContaAsaas.Aprovada,
        "REJECTED" or "DENIED" => StatusContaAsaas.Recusada,
        _ => StatusContaAsaas.Pendente
      };

    private static string NormalizarDocumento(string doc)
      => new string(doc.Where(char.IsDigit).ToArray());
  }
}
