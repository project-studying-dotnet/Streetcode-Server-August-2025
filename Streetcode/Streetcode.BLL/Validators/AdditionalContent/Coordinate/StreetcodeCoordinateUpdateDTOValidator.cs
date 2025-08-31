using FluentValidation;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class StreetcodeCoordinateUpdateDTOValidator : AbstractValidator<StreetcodeCoordinateDTO>
{
    public StreetcodeCoordinateUpdateDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be provided for update and greater than 0.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longtitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.StreetcodeId)
            .GreaterThan(0)
            .WithMessage("StreetcodeId must be greater than 0.");
    }
}