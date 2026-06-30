using FluentValidation;
using Libify.API.Dto;

namespace Libify.API.Validation
{
    public class ServicoDtoValidator : AbstractValidator<ServicoDto>
    {
        public ServicoDtoValidator()
        {
            RuleFor(s => s.Descricao)
                .NotEmpty().WithMessage("Descrição é obrigatória.")
                .MaximumLength(200);

            RuleFor(s => s.PrecoUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("Preço unitário não pode ser negativo.");

            RuleFor(s => s.Unidade)
                .MaximumLength(20);
        }
    }
}
