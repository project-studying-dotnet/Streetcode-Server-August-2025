using FluentValidation;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Media.Image.Art;

public class ArtCreateUpdateDTOValidator : AbstractValidator<ArtCreateUpdateDTO>
{
    public const int MaxTitleLength = 150;
    public const int MaxDescriptionLength = 400;

    public ArtCreateUpdateDTOValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(MaxTitleLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength));

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Description", MaxDescriptionLength));

        RuleFor(x => x.ModelState)
            .IsInEnum()
            .WithMessage(Errors_Validation.Invalid.FormatWith("ModelState"));
    }
}
