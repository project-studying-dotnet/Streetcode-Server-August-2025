using FluentValidation;
using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.BLL.Validators.Media.Image;

public class ImageFileBaseCreateDTOValidator
    : FileBaseCreateDTOValidator<ImageFileBaseCreateDTO>
{
    public ImageFileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Alt)
            .MaximumLength(200)
            .WithMessage("Alt text cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Alt));
    }
}