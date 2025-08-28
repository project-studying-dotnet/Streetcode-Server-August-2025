using FluentValidation;
using Streetcode.BLL.DTO.Media;
using System.Text.RegularExpressions;

namespace Streetcode.BLL.Validators.Media;

public class FileBaseCreateDTOValidator<T> : AbstractValidator<T>
    where T : FileBaseCreateDTO
{
    public FileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-z]+\/[a-z0-9.+-]+$", RegexOptions.IgnoreCase)
            .WithMessage("MimeType must match type/subtype (e.g., image/png, application/pdf).");

        RuleFor(x => x.Extension)
            .MaximumLength(10)
            .WithMessage("Extension cannot exceed 10 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Extension));

        RuleFor(x => x.BaseFormat)
            .MaximumLength(20)
            .WithMessage("Base format cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BaseFormat));
    }
}