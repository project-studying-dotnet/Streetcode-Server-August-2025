using FluentValidation;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Media.Audio;

public class AudioFileBaseCreateDTOValidator
    : FileBaseCreateDTOValidator<AudioFileBaseCreateDTO>
{
    public AudioFileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Description", 500))
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}