using FluentValidation;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Partners;

public class StreetcodeShortDTOValidator : AbstractValidator<StreetcodeShortDTO>
{
    public const int MaxTitleLength = 255;

    public StreetcodeShortDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.Invalid.FormatWith("Id"));

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Title"))
            .MaximumLength(MaxTitleLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength));
    }
}