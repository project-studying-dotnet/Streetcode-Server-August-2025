using FluentValidation;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.ArtGallery;

public class StreetcodeArtSlideValidator : AbstractValidator<StreetcodeArtSlideCreateUpdateDTO>
{
    public StreetcodeArtSlideValidator()
    {
        RuleFor(x => x.Template)
            .IsInEnum()
            .WithMessage(Errors_Validation.Invalid.FormatWith("Template"));

        RuleFor(x => x.StreetcodeArts)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("StreetcodeArts"));
    }
}
