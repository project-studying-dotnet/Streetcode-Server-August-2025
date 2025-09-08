using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class CreateCoordinateCommandValidator : AbstractValidator<CreateCoordinateCommand>
{
    public CreateCoordinateCommandValidator()
    {
        RuleFor(x => x.StreetcodeCoordinate)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("StreetcodeCoordinate"));

        When(x => x.StreetcodeCoordinate != null, () =>
        {
            RuleFor(x => x.StreetcodeCoordinate)
                .SetValidator(new StreetcodeCoordinateDTOValidator());
        });
    }
}