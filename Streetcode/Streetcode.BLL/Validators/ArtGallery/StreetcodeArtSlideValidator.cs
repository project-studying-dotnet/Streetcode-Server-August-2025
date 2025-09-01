using FluentValidation;
using Streetcode.BLL.DTO.ArtGallery;

namespace Streetcode.BLL.Validators.ArtGallery;

public class StreetcodeArtSlideValidator : AbstractValidator<StreetcodeArtSlideCreateUpdateDTO>
{
    public StreetcodeArtSlideValidator()
    {
        RuleFor(x => x.Template)
            .IsInEnum()
            .WithMessage("Invalid Template value.");

        RuleFor(x => x.StreetcodeArts)
            .NotEmpty()
            .WithMessage("StreetcodeArts collection cannot be empty.");
    }
}
