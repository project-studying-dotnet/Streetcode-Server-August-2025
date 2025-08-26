using FluentValidation;
using Streetcode.BLL.DTO.Media.Audio;

namespace Streetcode.BLL.Validators.Media.Audio;

public class AudioFileBaseCreateDTOValidator : AbstractValidator<AudioFileBaseCreateDTO>
{
    public AudioFileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage("MimeType is required.")
            .MaximumLength(10)
            .WithMessage("MimeType cannot exceed 10 characters.");

        RuleFor(x => x.Extension)
            .MaximumLength(10)
            .WithMessage("Extension cannot exceed 10 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Extension));

        RuleFor(x => x.BaseFormat)
            .MaximumLength(20)
            .WithMessage("Base format cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BaseFormat));

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}