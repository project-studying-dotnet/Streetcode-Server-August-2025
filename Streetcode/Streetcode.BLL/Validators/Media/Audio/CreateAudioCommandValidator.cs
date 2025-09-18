using FluentValidation;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Media.Audio;

public class CreateAudioCommandValidator : AbstractValidator<CreateAudioCommand>
{
    public CreateAudioCommandValidator()
    {
        RuleFor(x => x.Audio)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("Audio"));

        When(x => x.Audio != null, () =>
        {
            RuleFor(x => x.Audio)
                .SetValidator(new AudioFileBaseCreateDTOValidator());
        });
    }
}