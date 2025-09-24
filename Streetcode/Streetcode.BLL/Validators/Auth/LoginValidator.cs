using FluentValidation;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Auth;

public class LoginValidator : AbstractValidator<UserLoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Login)
                .Matches(@"^(?!.*\.\.)[a-zA-Z0-9_%+-]+(?:\.[a-zA-Z0-9_%+-]+)*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage(Errors_Validation.EmailAddressFormat.FormatWith("Login"))
                .NotEmpty().WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Login"));

        RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Password"));
    }
}
