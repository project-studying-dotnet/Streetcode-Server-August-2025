using FluentValidation;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Media.Image;

public class ImageFileBaseCreateDTOValidator
    : FileBaseCreateDTOValidator<ImageFileBaseCreateDTO>
{
    public ImageFileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Alt)
            .MaximumLength(200)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Alt", 200))
            .When(x => !string.IsNullOrWhiteSpace(x.Alt));
    }
}