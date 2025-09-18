using FluentValidation;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class StreetcodeCoordinateDTOValidator : AbstractValidator<StreetcodeCoordinateDTO>
{
    public StreetcodeCoordinateDTOValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage(Errors_Validation.MustBeBetween.FormatWith("Latitude", -90, 90));

        RuleFor(x => x.Longtitude)
            .InclusiveBetween(-180, 180)
            .WithMessage(Errors_Validation.MustBeBetween.FormatWith("Longtitude", -180, 180));

        RuleFor(x => x.StreetcodeId)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.GreaterThan.FormatWith("StreetcodeId", 0));

        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("Id must not be set when creating a new coordinate.");
    }
}