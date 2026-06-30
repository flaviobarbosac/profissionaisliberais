using FluentValidation;
using Libify.API.Dto;

namespace Libify.API.Validation
{
    public class PropostaDtoValidator : AbstractValidator<PropostaDto>
    {
        public PropostaDtoValidator()
        {
            RuleFor(p => p.ClienteId)
                .NotEmpty().WithMessage("Cliente é obrigatório.");

            RuleFor(p => p.Titulo)
                .MaximumLength(150);

            RuleFor(p => p.ValorTotal)
                .GreaterThanOrEqualTo(0).WithMessage("Valor total não pode ser negativo.");

            RuleForEach(p => p.Itens).SetValidator(new PropostaItemDtoValidator());
        }
    }

    public class PropostaItemDtoValidator : AbstractValidator<PropostaItemDto>
    {
        public PropostaItemDtoValidator()
        {
            RuleFor(i => i.Descricao)
                .NotEmpty().WithMessage("Descrição do item é obrigatória.")
                .MaximumLength(200);

            RuleFor(i => i.Quantidade)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

            RuleFor(i => i.ValorUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("Valor unitário não pode ser negativo.");
        }
    }
}
