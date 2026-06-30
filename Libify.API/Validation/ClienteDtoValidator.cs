using FluentValidation;
using Libify.API.Dto;

namespace Libify.API.Validation
{
    public class ClienteDtoValidator : AbstractValidator<ClienteDto>
    {
        public ClienteDtoValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(150);

            RuleFor(c => c.Email)
                .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email))
                .WithMessage("E-mail inválido.");

            RuleFor(c => c.CpfCnpj)
                .MaximumLength(18);
        }
    }
}
