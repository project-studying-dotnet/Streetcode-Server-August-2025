using FluentValidation;
using Streetcode.BLL.MediatR.Email;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Email;

public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("Email"));

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email)
                .SetValidator(new EmailDTOValidator());
        });
    }
}
