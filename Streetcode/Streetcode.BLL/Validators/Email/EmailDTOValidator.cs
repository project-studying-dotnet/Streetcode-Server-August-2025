using FluentValidation;
using Streetcode.BLL.DTO.Email;

namespace Streetcode.BLL.Validators.Email;

public class EmailDTOValidator : AbstractValidator<EmailDTO>
{
    public EmailDTOValidator()
    {
        RuleFor(x => x.From)
            .MaximumLength(80)
            .WithMessage("Sender address cannot exceed 80 characters.")
            .EmailAddress()
            .WithMessage("Sender must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.From));

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Email content is required.")
            .MinimumLength(1)
            .WithMessage("Email content must contain at least 1 character.")
            .MaximumLength(500)
            .WithMessage("Email content cannot exceed 500 characters.");
    }
}