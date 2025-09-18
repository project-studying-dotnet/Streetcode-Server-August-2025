using FluentValidation;
using Streetcode.BLL.MediatR.Team.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Team.Position;

public class CreatePositionQueryValidator : AbstractValidator<CreatePositionQuery>
{
    public const int MaxPositionLength = 50;

    public CreatePositionQueryValidator()
    {
        RuleFor(x => x.position)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("Position"))
            .DependentRules(() =>
            {
                RuleFor(x => x.position!.Position)
                    .NotEmpty()
                    .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Position"))
                    .MaximumLength(MaxPositionLength)
                    .WithMessage(Errors_Validation.MaxLength.FormatWith("Position", MaxPositionLength));
            });
    }
}