using FluentValidation;
using Streetcode.BLL.DTO.Media.Audio;

namespace Streetcode.BLL.Validators.Media.Audio;

public class AudioFileBaseCreateDTOValidator
    : FileBaseCreateDTOValidator<AudioFileBaseCreateDTO>
{
    public AudioFileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}