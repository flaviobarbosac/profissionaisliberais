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
  public class PlanoAssinaturaService : IPlanoAssinaturaService
  {
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IAsaasClient _asaas;
    private readonly IBaseRepository<Plano> _planos;
    private readonly AsaasConfig _config;

    public PlanoAssinaturaService(
      AppDbContext db,
      ITenantContext tenant,
      IAsaasClient asaas,
      IBaseRepository<Plano> planos,
      AsaasConfig config)
    {
      _db = db;
      _tenant = tenant;
      _asaas = asaas;
      _planos = planos;
      _config = config;
    }

    public async Task<Plano> AssinarAsync(AssinarPlanoRequest request, CancellationToken cancellationToken = default)
    {
      if (request.Tipo is TipoPlano.Gratuito)
        throw new InvalidOperationException("Plano gratuito não requer assinatura.");

      var usuario = await ObterUsuarioAsync(cancellationToken);
      usuario = await GarantirCustomerPlataformaAsync(usuario, cancellationToken);

      var ativo = await _db.Plano.FirstOrDefaultAsync(p => p.Ativo, cancellationToken);
      if (ativo is not null)
        throw new InvalidOperationException("Já existe uma assinatura ativa.");

      var valor = ObterValor(request.Tipo);
      var ciclo = AsaasStatusMapper.MapearCicloAssinatura(request.Tipo);
      var billing = AsaasStatusMapper.MapearFormaPagamento(request.FormaPagamento);

      var assinatura = await _asaas.CriarAssinaturaAsync(
        _config.PlatformApiKey,
        new AsaasAssinaturaRequest(
          Customer: usuario.AsaasPlatformCustomerId!,
          BillingType: billing,
          Value: valor,
          Cycle: ciclo,
          NextDueDate: DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
          Description: $"Libify Premium - {request.Tipo}"),
        cancellationToken);

      var agora = DateTimeHelper.UtcNow;
      var plano = new Plano
      {
        Tipo = request.Tipo,
        Valor = valor,
        InicioEm = agora,
        Ativo = true,
        AsaasSubscriptionId = assinatura.Id
      };

      usuario.Plano = request.Tipo;
      usuario.PlanoValidoAte = CalcularValidade(request.Tipo, agora);
      usuario.UpdatedAt = agora;
      usuario.Version += 1;

      await _planos.AddAsync(plano);
      await _db.SaveChangesAsync(cancellationToken);
      return plano;
    }

    public async Task CancelarAsync(CancellationToken cancellationToken = default)
    {
      var plano = await _db.Plano.FirstOrDefaultAsync(p => p.Ativo, cancellationToken)
        ?? throw new InvalidOperationException("Nenhuma assinatura ativa.");

      if (!string.IsNullOrWhiteSpace(plano.AsaasSubscriptionId))
        await _asaas.CancelarAssinaturaAsync(_config.PlatformApiKey, plano.AsaasSubscriptionId, cancellationToken);

      var agora = DateTimeHelper.UtcNow;
      plano.Ativo = false;
      plano.FimEm = agora;
      await _planos.UpdateAsync(plano);

      var usuario = await ObterUsuarioAsync(cancellationToken);
      usuario.Plano = TipoPlano.Gratuito;
      usuario.PlanoValidoAte = null;
      usuario.UpdatedAt = agora;
      usuario.Version += 1;
      await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> ObterUsuarioAsync(CancellationToken cancellationToken)
    {
      if (!_tenant.UsuarioId.HasValue)
        throw new UnauthorizedAccessException("Usuário não autenticado.");

      return await _db.Usuario.FirstOrDefaultAsync(u => u.Id == _tenant.UsuarioId.Value, cancellationToken)
        ?? throw new InvalidOperationException("Usuário não encontrado.");
    }

    private async Task<Usuario> GarantirCustomerPlataformaAsync(Usuario usuario, CancellationToken cancellationToken)
    {
      if (!string.IsNullOrWhiteSpace(usuario.AsaasPlatformCustomerId))
        return usuario;

      if (string.IsNullOrWhiteSpace(usuario.CpfCnpj))
        throw new InvalidOperationException("CPF/CNPJ é obrigatório para assinar o plano.");

      var doc = new string(usuario.CpfCnpj.Where(char.IsDigit).ToArray());
      var customer = await _asaas.CriarClienteAsync(
        _config.PlatformApiKey,
        new AsaasClienteRequest(usuario.Nome, doc, usuario.Email, usuario.Telefone),
        cancellationToken);

      usuario.AsaasPlatformCustomerId = customer.Id;
      usuario.UpdatedAt = DateTimeHelper.UtcNow;
      usuario.Version += 1;
      await _db.SaveChangesAsync(cancellationToken);
      return usuario;
    }

    private decimal ObterValor(TipoPlano tipo) => tipo switch
    {
      TipoPlano.Mensal => _config.ValorPlanoMensal,
      TipoPlano.Semestral => _config.ValorPlanoSemestral,
      TipoPlano.Anual => _config.ValorPlanoAnual,
      _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private static DateTime CalcularValidade(TipoPlano tipo, DateTime inicio) => tipo switch
    {
      TipoPlano.Mensal => inicio.AddMonths(1),
      TipoPlano.Semestral => inicio.AddMonths(6),
      TipoPlano.Anual => inicio.AddYears(1),
      _ => inicio
    };
  }
}
