using FluentValidation;
using Streetcode.BLL.MediatR.Team.Create;

namespace Streetcode.BLL.Validators.Team.Position;

public class CreatePositionQueryValidator : AbstractValidator<CreatePositionQuery>
{
    public CreatePositionQueryValidator()
    {
        RuleFor(x => x.position)
            .NotNull()
            .WithMessage("Position object is required.");

        RuleFor(x => x.position.Position)
            .NotEmpty()
            .WithMessage("Position name is required.")
            .MaximumLength(50)
            .WithMessage("Position name cannot exceed 50 characters.");
    }
}