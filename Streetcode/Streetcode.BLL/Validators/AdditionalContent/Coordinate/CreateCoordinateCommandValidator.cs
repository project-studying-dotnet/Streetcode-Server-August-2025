using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class CreateCoordinateCommandValidator : AbstractValidator<CreateCoordinateCommand>
{
    public CreateCoordinateCommandValidator()
    {
        RuleFor(x => x.StreetcodeCoordinate)
            .NotNull()
            .WithMessage("Coordinate data is required.");

        When(x => x.StreetcodeCoordinate != null, () =>
        {
            RuleFor(x => x.StreetcodeCoordinate)
                .SetValidator(new StreetcodeCoordinateDTOValidator());
        });
    }
}