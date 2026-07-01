using FluentValidation;
using Libify.API.Dto;

namespace Libify.API.Validation
{
  public class ResponderPropostaPortalDtoValidator : AbstractValidator<ResponderPropostaPortalDto>
  {
    public ResponderPropostaPortalDtoValidator()
    {
      RuleFor(r => r.Nome).MaximumLength(150);
    }
  }

  public class RecusarPropostaPortalDtoValidator : AbstractValidator<RecusarPropostaPortalDto>
  {
    public RecusarPropostaPortalDtoValidator()
    {
      RuleFor(r => r.Nome).MaximumLength(150);
      RuleFor(r => r.Motivo).MaximumLength(500);
    }
  }
}
