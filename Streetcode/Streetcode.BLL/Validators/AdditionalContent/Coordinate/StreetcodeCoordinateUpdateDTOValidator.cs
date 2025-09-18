using FluentValidation;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.AdditionalContent.Coordinate;

public class StreetcodeCoordinateUpdateDTOValidator : AbstractValidator<StreetcodeCoordinateDTO>
{
    public StreetcodeCoordinateUpdateDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.GreaterThan.FormatWith("Id", 0));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage(Errors_Validation.MustBeBetween.FormatWith("Latitude", -90, 90));

        RuleFor(x => x.Longtitude)
            .InclusiveBetween(-180, 180)
            .WithMessage(Errors_Validation.MustBeBetween.FormatWith("Longtitude", -180, 180));
        RuleFor(x => x.StreetcodeId)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.GreaterThan.FormatWith("StreetcodeId", 0));
    }
}