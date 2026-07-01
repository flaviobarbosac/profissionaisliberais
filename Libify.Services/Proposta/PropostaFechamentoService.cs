using Libify.Domain.Enum;
using Libify.Domain.Helpers;
using Libify.Domain.Messaging;
using Libify.Domain.Model;
using Libify.Domain.Model.Base;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Services.Asaas;
using Microsoft.EntityFrameworkCore;

namespace Libify.Services.Proposta
{
  public record FechamentoAceiteResult(
    Guid ContratoId,
    Guid? CobrancaId,
    string? LinkPagamento,
    string? PixCopiaECola,
    bool CobrancaAsaasEmitida,
    string? AvisoAsaas);

  public interface IPropostaFechamentoService
  {
    Task<FechamentoAceiteResult> ConcluirAceiteAsync(
      Domain.Model.Proposta proposta,
      string? aceitoPor,
      string? ipAceite,
      CancellationToken cancellationToken = default);
  }

  /// <summary>
  /// Ao aceitar proposta no portal: gera contrato assinado e tenta cobrança Asaas (fallback local).
  /// </summary>
  public class PropostaFechamentoService : IPropostaFechamentoService
  {
    private readonly AppDbContext _db;
    private readonly IMessageBus _bus;
    private readonly ICobrancaAsaasService _cobrancaAsaas;
    private readonly ITenantContext _tenant;

    public PropostaFechamentoService(
      AppDbContext db,
      IMessageBus bus,
      ICobrancaAsaasService cobrancaAsaas,
      ITenantContext tenant)
    {
      _db = db;
      _bus = bus;
      _cobrancaAsaas = cobrancaAsaas;
      _tenant = tenant;
    }

    public async Task<FechamentoAceiteResult> ConcluirAceiteAsync(
      Domain.Model.Proposta proposta,
      string? aceitoPor,
      string? ipAceite,
      CancellationToken cancellationToken = default)
    {
      var agora = DateTimeHelper.UtcNow;

      var contratoExistente = await _db.Contrato
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(c => c.PropostaId == proposta.Id && c.DeletedAt == null, cancellationToken);

      Contrato contrato;
      if (contratoExistente is not null)
      {
        contrato = contratoExistente;
        contrato.Aceito = true;
        contrato.AceitoEm = agora;
        contrato.AceitoPor = aceitoPor;
        contrato.IpAceite = ipAceite;
        contrato.Status = ContratoStatus.Assinado;
        contrato.UpdatedAt = agora;
        contrato.Version += 1;
      }
      else
      {
        contrato = new Contrato
        {
          Id = Guid.CreateVersion7(),
          UsuarioId = proposta.UsuarioId,
          PropostaId = proposta.Id,
          Status = ContratoStatus.Assinado,
          Aceito = true,
          AceitoEm = agora,
          AceitoPor = aceitoPor,
          IpAceite = ipAceite,
          CreatedAt = agora,
          UpdatedAt = agora,
          Version = 1
        };
        _db.Contrato.Add(contrato);
        await PublicarSyncAsync(contrato, proposta.UsuarioId, cancellationToken);
      }

      Cobranca? cobranca = null;
      string? aviso = null;
      var asaasOk = false;

      var cobrancaExistente = await _db.Cobranca
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(c => c.PropostaId == proposta.Id && c.DeletedAt == null, cancellationToken);

      if (cobrancaExistente is not null)
      {
        cobranca = cobrancaExistente;
        asaasOk = !string.IsNullOrWhiteSpace(cobranca.AsaasPaymentId);
      }
      else
      {
        _tenant.Set(proposta.UsuarioId, null);
        try
        {
          cobranca = await _cobrancaAsaas.EmitirAsync(new EmitirCobrancaRequest(
            proposta.ClienteId,
            proposta.ValorTotal,
            agora.AddDays(7),
            FormaPagamento.Pix,
            proposta.Id,
            1,
            proposta.Titulo ?? "Cobranca referente a proposta aceita"), cancellationToken);
          asaasOk = true;
        }
        catch (Exception ex)
        {
          aviso = ex.Message;
          cobranca = new Cobranca
          {
            Id = Guid.CreateVersion7(),
            UsuarioId = proposta.UsuarioId,
            ClienteId = proposta.ClienteId,
            PropostaId = proposta.Id,
            FormaPagamento = FormaPagamento.Pix,
            Status = StatusCobranca.Pendente,
            Valor = proposta.ValorTotal,
            Vencimento = agora.AddDays(7),
            Parcelas = 1,
            Descricao = proposta.Titulo ?? "Cobranca referente a proposta aceita",
            CreatedAt = agora,
            UpdatedAt = agora,
            Version = 1
          };
          _db.Cobranca.Add(cobranca);
          await PublicarSyncAsync(cobranca, proposta.UsuarioId, cancellationToken);
        }
      }

      await _db.SaveChangesAsync(cancellationToken);

      return new FechamentoAceiteResult(
        contrato.Id,
        cobranca?.Id,
        cobranca?.LinkPagamento,
        cobranca?.PixCopiaECola,
        asaasOk,
        aviso);
    }

    private async Task PublicarSyncAsync<T>(T entity, Guid usuarioId, CancellationToken ct)
      where T : ModelBase, ITenantOwned
    {
      await _bus.PublishAsync(new SyncEvent<T>
      {
        EntityId = entity.Id,
        UsuarioId = usuarioId,
        Version = entity.Version,
        Operacao = SyncOperacao.Upsert,
        Payload = entity,
        OcorridoEm = DateTimeHelper.UtcNow
      }, ct);
    }
  }
}
