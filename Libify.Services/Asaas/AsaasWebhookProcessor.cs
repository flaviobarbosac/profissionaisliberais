using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Libify.Domain.Enum;
using Libify.Domain.Helpers;
using Libify.Domain.Model;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Libify.Services.Asaas
{
  public class AsaasWebhookProcessor : IAsaasWebhookProcessor
  {
    private readonly AppDbContext _db;
    private readonly AsaasConfig _config;

    public AsaasWebhookProcessor(AppDbContext db, AsaasConfig config)
    {
      _db = db;
      _config = config;
    }

    public async Task<string> ProcessarAsync(string payload, string? authToken, CancellationToken cancellationToken = default)
    {
      if (!string.IsNullOrWhiteSpace(_config.WebhookAuthToken)
          && !string.Equals(authToken, _config.WebhookAuthToken, StringComparison.Ordinal))
        throw new UnauthorizedAccessException("Token de webhook inválido.");

      using var doc = JsonDocument.Parse(payload);
      var root = doc.RootElement;

      var eventType = root.TryGetProperty("event", out var ev) ? ev.GetString() ?? "UNKNOWN" : "UNKNOWN";
      var eventId = ExtrairEventId(root, eventType, payload);

      var existente = await _db.AsaasWebhookEvent.IgnoreQueryFilters()
        .FirstOrDefaultAsync(e => e.EventId == eventId && e.DeletedAt == null, cancellationToken);

      if (existente?.ProcessadoEm is not null)
        return "ignorado";

      var agora = DateTimeHelper.UtcNow;
      if (existente is null)
      {
        existente = new AsaasWebhookEvent
        {
          Id = Guid.CreateVersion7(),
          EventId = eventId,
          EventType = eventType,
          Payload = payload,
          CreatedAt = agora,
          UpdatedAt = agora,
          Version = 1
        };
        _db.AsaasWebhookEvent.Add(existente);
      }

      try
      {
        if (eventType.StartsWith("PAYMENT_", StringComparison.OrdinalIgnoreCase))
          await ProcessarPagamentoAsync(root, cancellationToken);
        else if (eventType.StartsWith("SUBSCRIPTION_", StringComparison.OrdinalIgnoreCase))
          await ProcessarAssinaturaAsync(root, eventType, cancellationToken);
        else
        {
          existente.Resultado = "ignorado";
          existente.ProcessadoEm = agora;
          existente.UpdatedAt = agora;
          await _db.SaveChangesAsync(cancellationToken);
          return "ignorado";
        }

        existente.Resultado = "ok";
      }
      catch (Exception ex)
      {
        existente.Resultado = $"erro:{ex.Message}";
        existente.UpdatedAt = agora;
        await _db.SaveChangesAsync(cancellationToken);
        throw;
      }

      existente.ProcessadoEm = agora;
      existente.UpdatedAt = agora;
      await _db.SaveChangesAsync(cancellationToken);
      return "ok";
    }

    private async Task ProcessarPagamentoAsync(JsonElement root, CancellationToken cancellationToken)
    {
      if (!root.TryGetProperty("payment", out var payment))
        return;

      var paymentId = payment.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
      if (string.IsNullOrWhiteSpace(paymentId))
        return;

      var cobranca = await _db.Cobranca.IgnoreQueryFilters()
        .FirstOrDefaultAsync(c => c.AsaasPaymentId == paymentId && c.DeletedAt == null, cancellationToken);

      if (cobranca is null)
        return;

      var statusRaw = payment.TryGetProperty("status", out var st) ? st.GetString() : null;
      var status = AsaasStatusMapper.MapearPagamento(statusRaw);
      if (status.HasValue)
        cobranca.Status = status.Value;

      cobranca.AsaasStatusRaw = statusRaw;
      cobranca.UpdatedAt = DateTimeHelper.UtcNow;
      cobranca.Version += 1;

      if (cobranca.Status == StatusCobranca.Recebida)
        await GarantirLancamentoRecebidoAsync(cobranca, cancellationToken);

      await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessarAssinaturaAsync(JsonElement root, string eventType, CancellationToken cancellationToken)
    {
      if (!root.TryGetProperty("subscription", out var sub))
        return;

      var subId = sub.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
      if (string.IsNullOrWhiteSpace(subId))
        return;

      var plano = await _db.Plano.IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.AsaasSubscriptionId == subId && p.DeletedAt == null, cancellationToken);

      if (plano is null)
        return;

      var agora = DateTimeHelper.UtcNow;
      if (eventType.Equals("SUBSCRIPTION_DELETED", StringComparison.OrdinalIgnoreCase))
      {
        plano.Ativo = false;
        plano.FimEm = agora;
      }
      else
      {
        plano.Ativo = true;
        plano.FimEm = null;
      }

      plano.UpdatedAt = agora;
      plano.Version += 1;

      var usuario = await _db.Usuario.IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Id == plano.UsuarioId && u.DeletedAt == null, cancellationToken);

      if (usuario is not null)
      {
        if (plano.Ativo)
        {
          usuario.Plano = plano.Tipo;
          usuario.PlanoValidoAte = plano.Tipo switch
          {
            TipoPlano.Mensal => agora.AddMonths(1),
            TipoPlano.Semestral => agora.AddMonths(6),
            TipoPlano.Anual => agora.AddYears(1),
            _ => usuario.PlanoValidoAte
          };
        }
        else
        {
          usuario.Plano = TipoPlano.Gratuito;
          usuario.PlanoValidoAte = null;
        }

        usuario.UpdatedAt = agora;
        usuario.Version += 1;
      }

      await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task GarantirLancamentoRecebidoAsync(Cobranca cobranca, CancellationToken cancellationToken)
    {
      var existe = await _db.LancamentoFinanceiro.IgnoreQueryFilters()
        .AnyAsync(l => l.CobrancaId == cobranca.Id && l.DeletedAt == null, cancellationToken);

      if (existe)
        return;

      var agora = DateTimeHelper.UtcNow;
      _db.LancamentoFinanceiro.Add(new LancamentoFinanceiro
      {
        Id = Guid.CreateVersion7(),
        UsuarioId = cobranca.UsuarioId,
        ClienteId = cobranca.ClienteId,
        CobrancaId = cobranca.Id,
        Tipo = TipoLancamento.Receber,
        Status = StatusLancamento.Pago,
        Descricao = cobranca.Descricao ?? "Recebimento via Asaas",
        Valor = cobranca.Valor,
        Vencimento = cobranca.Vencimento,
        PagoEm = agora,
        CreatedAt = agora,
        UpdatedAt = agora,
        Version = 1
      });
    }

    private static string ExtrairEventId(JsonElement root, string eventType, string payload)
    {
      if (root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
        return id.GetString()!;

      if (root.TryGetProperty("payment", out var pay) && pay.TryGetProperty("id", out var payId))
        return $"{eventType}:{payId.GetString()}";

      if (root.TryGetProperty("subscription", out var sub) && sub.TryGetProperty("id", out var subId))
        return $"{eventType}:{subId.GetString()}";

      var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
      return $"{eventType}:{Convert.ToHexString(hash)[..16]}";
    }
  }
}
