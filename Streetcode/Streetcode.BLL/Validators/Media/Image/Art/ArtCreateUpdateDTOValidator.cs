using FluentValidation;
using Streetcode.BLL.DTO.Media.Art;

namespace Streetcode.BLL.Validators.Media.Image.Art;

public class ArtCreateUpdateDTOValidator : AbstractValidator<ArtCreateUpdateDTO>
{
    public const int MaxTitleLength = 150;
    public const int MaxDescriptionLength = 400;

    public ArtCreateUpdateDTOValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(MaxTitleLength)
            .WithMessage($"Title cannot exceed {MaxTitleLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Description cannot exceed {MaxDescriptionLength} characters.");

        RuleFor(x => x.ModelState)
            .IsInEnum()
            .WithMessage("Invalid ModelState value.");
    }
}
