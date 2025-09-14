using FluentValidation;
using Streetcode.BLL.MediatR.Users.Register;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Users;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.registeredUserDto)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("RegisteredUserDto"));

        When(x => x.registeredUserDto != null, () =>
        {
            RuleFor(x => x.registeredUserDto)
                .SetValidator(new RegisterUserDTOValidator());
        });
    }
}