using FluentValidation;
using Libify.API.Dto;

namespace Libify.API.Validation
{
    public class CobrancaDtoValidator : AbstractValidator<CobrancaDto>
    {
        public CobrancaDtoValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty().WithMessage("Cliente é obrigatório.");

            RuleFor(c => c.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");

            RuleFor(c => c.Vencimento)
                .NotEmpty().WithMessage("Vencimento é obrigatório.");

            RuleFor(c => c.Parcelas)
                .GreaterThanOrEqualTo(1).WithMessage("Parcelas deve ser no mínimo 1.");
        }
    }
}
