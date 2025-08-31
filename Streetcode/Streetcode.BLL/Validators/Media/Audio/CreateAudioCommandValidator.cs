using FluentValidation;
using Streetcode.BLL.MediatR.Media.Audio.Create;

namespace Streetcode.BLL.Validators.Media.Audio;

public class CreateAudioCommandValidator : AbstractValidator<CreateAudioCommand>
{
    public CreateAudioCommandValidator()
    {
        RuleFor(x => x.Audio)
            .NotNull()
            .WithMessage("Audio data is required.");

        When(x => x.Audio != null, () =>
        {
            RuleFor(x => x.Audio)
                .SetValidator(new AudioFileBaseCreateDTOValidator());
        });
    }
}