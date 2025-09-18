using FluentValidation;
using Streetcode.BLL.DTO.Email;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Email;

public class EmailDTOValidator : AbstractValidator<EmailDTO>
{
    public EmailDTOValidator()
    {
        RuleFor(x => x.From)
            .MaximumLength(80)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("From", 80))
            .EmailAddress()
            .WithMessage(Errors_Validation.EmailAddressFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.From));

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Content"))
            .Length(1, 500)
            .WithMessage(Errors_Validation.LengthMustBeInRange.FormatWith("Content", 1, 500));
    }
}