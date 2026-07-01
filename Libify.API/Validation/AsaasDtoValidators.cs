using FluentValidation;
using Libify.API.Dto;
using Libify.Domain.Enum;

namespace Libify.API.Validation
{
  public class EmitirCobrancaDtoValidator : AbstractValidator<EmitirCobrancaDto>
  {
    public EmitirCobrancaDtoValidator()
    {
      RuleFor(c => c.ClienteId).NotEmpty();
      RuleFor(c => c.Valor).GreaterThan(0);
      RuleFor(c => c.Vencimento).NotEmpty();
      RuleFor(c => c.Parcelas).GreaterThanOrEqualTo(1);
      RuleFor(c => c.FormaPagamento)
        .Must(f => f is FormaPagamento.Pix or FormaPagamento.Boleto or FormaPagamento.CartaoCredito)
        .WithMessage("Forma de pagamento inválida para emissão.");
    }
  }

  public class AssinarPlanoDtoValidator : AbstractValidator<AssinarPlanoDto>
  {
    public AssinarPlanoDtoValidator()
    {
      RuleFor(p => p.Tipo)
        .Must(t => t is TipoPlano.Mensal or TipoPlano.Semestral or TipoPlano.Anual)
        .WithMessage("Tipo de plano inválido para assinatura.");
    }
  }
}
