using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.MediatR.Users.ChangePassword;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Users.ChangeUserPasswordValidator;

namespace Streetcode.BLL.Validators.Users.ChangeUserPasswordCommandValidator
{
    public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangeUserPasswordCommandValidator()
        {
            RuleFor(x => x.changePasswordRequestDto)
                .NotNull()
                .WithMessage(Errors_Validation.IsRequiredData.FormatWith("ChangePasswordRequestDto"));

            When(x => x.changePasswordRequestDto != null, () =>
            {
                RuleFor(x => x.changePasswordRequestDto)
                    .SetValidator(new ChangeUserPasswordDtoValidator());
            });
        }
    }
}
