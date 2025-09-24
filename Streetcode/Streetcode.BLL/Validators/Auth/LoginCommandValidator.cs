using FluentValidation;
using Streetcode.BLL.MediatR.Auth.Login;

namespace Streetcode.BLL.Validators.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(LoginValidator loginValidator)
    {
        RuleFor(dto => dto.userLoginDTO).SetValidator(loginValidator);
    }
}
