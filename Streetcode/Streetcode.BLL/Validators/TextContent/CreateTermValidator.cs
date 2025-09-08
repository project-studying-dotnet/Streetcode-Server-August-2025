using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;

namespace Streetcode.BLL.Validator.Streetcode.Term.Create;

public sealed class CreateTermValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermValidator()
    {
        RuleFor(cmd => cmd.Term.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(cmd => cmd.Term.Description)
            .NotEmpty();

        RuleFor(cmd => cmd.Term.StreetcodeId)
            .GreaterThan(0);
    }
}
