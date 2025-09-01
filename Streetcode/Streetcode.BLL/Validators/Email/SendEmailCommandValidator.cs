using FluentValidation;
using Streetcode.BLL.MediatR.Email;

namespace Streetcode.BLL.Validators.Email;

public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage("Email data is required.");

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email)
                .SetValidator(new EmailDTOValidator());
        });
    }
}
