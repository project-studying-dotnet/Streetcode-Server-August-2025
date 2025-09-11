using System.Text.RegularExpressions;
using FluentValidation;
using Streetcode.BLL.DTO.Media;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Media;

public class FileBaseCreateDTOValidator<T> : AbstractValidator<T>
    where T : FileBaseCreateDTO
{
    public const int MaxTitleLength = 100;
    public const int MaxMimeTypeLength = 50;
    public const int MaxExtensionLength = 10;
    public const int MaxBaseFormatLength = 20;
    public const string MimeTypePattern = @"^[a-z]+\/[a-z0-9.+-]+$";

    public FileBaseCreateDTOValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(MaxTitleLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength))
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("MimeType"))
            .MaximumLength(MaxMimeTypeLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("MimeType", MaxMimeTypeLength))
            .Matches(MimeTypePattern, RegexOptions.IgnoreCase)
            .WithMessage(Errors_Validation.MustMatchPattern.FormatWith("MimeType", MimeTypePattern));

        RuleFor(x => x.Extension)
            .MaximumLength(MaxExtensionLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Extension", MaxExtensionLength))
            .When(x => !string.IsNullOrWhiteSpace(x.Extension));

        RuleFor(x => x.BaseFormat)
            .MaximumLength(MaxBaseFormatLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("BaseFormat", MaxBaseFormatLength))
            .When(x => !string.IsNullOrWhiteSpace(x.BaseFormat));
    }
}