using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class UpdateCoordinateCommandValidator : AbstractValidator<UpdateCoordinateCommand>
{
    public UpdateCoordinateCommandValidator()
    {
        RuleFor(x => x.StreetcodeCoordinate)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("StreetcodeCoordinate"));

        When(x => x.StreetcodeCoordinate != null, () =>
        {
            RuleFor(x => x.StreetcodeCoordinate)
                .SetValidator(new StreetcodeCoordinateUpdateDTOValidator());
        });
    }
}