using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Users.ChangePassword;

namespace Streetcode.BLL.Validators.Users.ChangeUserPasswordValidator
{
    public class ChangeUserPasswordDtoValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public const int MinPasswordLength = 6;
        public ChangeUserPasswordDtoValidator()
        {
            RuleFor(x => x.NewPassword)
             .NotEmpty().WithMessage("Password is required")
            .MinimumLength(MinPasswordLength).WithMessage($"Password must be at least {MinPasswordLength} characters long")
            .MaximumLength(20).WithMessage("Password must not exceed 20 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
        }
    }
}
