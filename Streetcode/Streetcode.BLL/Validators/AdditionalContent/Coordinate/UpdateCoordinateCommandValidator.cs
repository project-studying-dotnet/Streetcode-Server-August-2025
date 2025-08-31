using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class UpdateCoordinateCommandValidator : AbstractValidator<UpdateCoordinateCommand>
{
    public UpdateCoordinateCommandValidator()
    {
        RuleFor(x => x.StreetcodeCoordinate)
            .NotNull()
            .WithMessage("Coordinate data is required.");

        When(x => x.StreetcodeCoordinate != null, () =>
        {
            RuleFor(x => x.StreetcodeCoordinate)
                .SetValidator(new StreetcodeCoordinateUpdateDTOValidator());
        });
    }
}