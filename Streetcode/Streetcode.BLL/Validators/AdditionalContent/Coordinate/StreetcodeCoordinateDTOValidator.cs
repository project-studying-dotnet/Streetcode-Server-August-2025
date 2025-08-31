using FluentValidation;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class StreetcodeCoordinateDTOValidator : AbstractValidator<StreetcodeCoordinateDTO>
{
    public StreetcodeCoordinateDTOValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longtitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.StreetcodeId)
            .GreaterThan(0)
            .WithMessage("StreetcodeId must be greater than 0.");

        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("Id must not be set when creating a new coordinate.");
    }
}