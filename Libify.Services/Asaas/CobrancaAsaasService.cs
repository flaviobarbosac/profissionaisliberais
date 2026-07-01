using Libify.Domain.Enum;
using Libify.Domain.Helpers;
using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Services;
using Libify.Infraestructure.Services.Interface;
using Libify.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Libify.Services.Asaas
{
  public class CobrancaAsaasService : ICobrancaAsaasService
  {
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IAsaasClient _asaas;
    private readonly ISecretProtector _protector;
    private readonly IBaseRepository<Cobranca> _cobrancas;

    public CobrancaAsaasService(
      AppDbContext db,
      ITenantContext tenant,
      IAsaasClient asaas,
      ISecretProtector protector,
      IBaseRepository<Cobranca> cobrancas)
    {
      _db = db;
      _tenant = tenant;
      _asaas = asaas;
      _protector = protector;
      _cobrancas = cobrancas;
    }

    public async Task<Cobranca> EmitirAsync(EmitirCobrancaRequest request, CancellationToken cancellationToken = default)
    {
      var usuario = await ObterUsuarioComSubcontaAsync(cancellationToken);
      var apiKey = _protector.Unprotect(usuario.AsaasApiKey!);

      var cliente = await _db.Cliente.FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken)
        ?? throw new InvalidOperationException("Cliente não encontrado.");

      if (string.IsNullOrWhiteSpace(cliente.CpfCnpj))
        throw new InvalidOperationException("Cliente precisa de CPF/CNPJ para cobrança Asaas.");

      cliente = await GarantirCustomerAsync(apiKey, cliente, cancellationToken);

      var billingType = AsaasStatusMapper.MapearFormaPagamento(request.FormaPagamento);
      if (billingType == "UNDEFINED")
        throw new InvalidOperationException("Forma de pagamento inválida.");

      var cobrancaLocalId = Guid.CreateVersion7();
      var asaasRequest = new AsaasCobrancaRequest(
        Customer: cliente.AsaasCustomerId!,
        BillingType: billingType,
        Value: request.Valor,
        DueDate: request.Vencimento.ToString("yyyy-MM-dd"),
        Description: request.Descricao,
        InstallmentCount: request.Parcelas > 1 ? request.Parcelas : null,
        ExternalReference: cobrancaLocalId.ToString());

      var pagamento = await _asaas.CriarCobrancaAsync(apiKey, asaasRequest, cancellationToken);
      if (string.IsNullOrWhiteSpace(pagamento.Id))
        throw new InvalidOperationException("Asaas não retornou o ID do pagamento.");

      string? pixQr = null;
      string? pixCopia = null;
      if (request.FormaPagamento == FormaPagamento.Pix)
      {
        var pix = await _asaas.ObterPixQrCodeAsync(apiKey, pagamento.Id, cancellationToken);
        pixQr = pix.EncodedImage;
        pixCopia = pix.Payload;
      }

      var status = AsaasStatusMapper.MapearPagamento(pagamento.Status) ?? StatusCobranca.Pendente;
      var cobranca = new Cobranca
      {
        Id = cobrancaLocalId,
        ClienteId = cliente.Id,
        PropostaId = request.PropostaId,
        FormaPagamento = request.FormaPagamento,
        Status = status,
        Valor = request.Valor,
        Vencimento = request.Vencimento,
        Parcelas = request.Parcelas,
        Descricao = request.Descricao,
        AsaasPaymentId = pagamento.Id,
        AsaasStatusRaw = pagamento.Status,
        PixQrCode = pixQr,
        PixCopiaECola = pixCopia,
        BoletoUrl = pagamento.BankSlipUrl,
        LinkPagamento = pagamento.InvoiceUrl
      };

      await _cobrancas.AddAsync(cobranca);
      return cobranca;
    }

    public async Task CancelarAsync(Guid cobrancaId, CancellationToken cancellationToken = default)
    {
      var usuario = await ObterUsuarioComSubcontaAsync(cancellationToken);
      var apiKey = _protector.Unprotect(usuario.AsaasApiKey!);

      var cobranca = await _db.Cobranca.FirstOrDefaultAsync(c => c.Id == cobrancaId, cancellationToken)
        ?? throw new InvalidOperationException("Cobrança não encontrada.");

      if (string.IsNullOrWhiteSpace(cobranca.AsaasPaymentId))
        throw new InvalidOperationException("Cobrança sem referência Asaas.");

      await _asaas.CancelarCobrancaAsync(apiKey, cobranca.AsaasPaymentId, cancellationToken);
      cobranca.Status = StatusCobranca.Cancelada;
      cobranca.AsaasStatusRaw = "DELETED";
      await _cobrancas.UpdateAsync(cobranca);
    }

    private async Task<Usuario> ObterUsuarioComSubcontaAsync(CancellationToken cancellationToken)
    {
      if (!_tenant.UsuarioId.HasValue)
        throw new UnauthorizedAccessException("Usuário não autenticado.");

      var usuario = await _db.Usuario.FirstOrDefaultAsync(u => u.Id == _tenant.UsuarioId.Value, cancellationToken)
        ?? throw new InvalidOperationException("Usuário não encontrado.");

      if (string.IsNullOrWhiteSpace(usuario.AsaasApiKey))
        throw new InvalidOperationException("Crie a subconta Asaas antes de emitir cobranças.");

      if (usuario.StatusContaAsaas == StatusContaAsaas.Recusada)
        throw new InvalidOperationException("Subconta Asaas recusada.");

      return usuario;
    }

    private async Task<Cliente> GarantirCustomerAsync(string apiKey, Cliente cliente, CancellationToken cancellationToken)
    {
      var doc = new string((cliente.CpfCnpj ?? "").Where(char.IsDigit).ToArray());
      var req = new AsaasClienteRequest(
        Name: cliente.Nome,
        CpfCnpj: doc,
        Email: cliente.Email,
        MobilePhone: MontarTelefone(cliente.Ddd, cliente.Telefone));

      if (string.IsNullOrWhiteSpace(cliente.AsaasCustomerId))
      {
        var criado = await _asaas.CriarClienteAsync(apiKey, req, cancellationToken);
        cliente.AsaasCustomerId = criado.Id;
      }
      else
      {
        await _asaas.AtualizarClienteAsync(apiKey, cliente.AsaasCustomerId, req, cancellationToken);
      }

      cliente.UpdatedAt = DateTimeHelper.UtcNow;
      cliente.Version += 1;
      await _db.SaveChangesAsync(cancellationToken);
      return cliente;
    }

    private static string? MontarTelefone(string? ddd, string? telefone)
    {
      if (string.IsNullOrWhiteSpace(telefone)) return null;
      return string.IsNullOrWhiteSpace(ddd) ? telefone : $"{ddd}{telefone}";
    }
  }
}
