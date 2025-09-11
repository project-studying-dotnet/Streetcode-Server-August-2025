using FluentValidation;
using Streetcode.BLL.DTO.Users;

namespace Streetcode.BLL.Validators.Users;

public class RegisterUserDTOValidator : AbstractValidator<RegisterUserDTO>
{
    public const int MinPasswordLength = 6;
    public RegisterUserDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(50).WithMessage("Email must not exceed 50 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(MinPasswordLength).WithMessage($"Password must be at least {MinPasswordLength} characters long")
            .MaximumLength(20).WithMessage("Password must not exceed 20 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");

        RuleFor(x => x.UserName)
            .MaximumLength(20).WithMessage("UserName must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        RuleFor(x => x.Name)
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Surname)
            .MaximumLength(50).WithMessage("Surname must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Surname));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{10,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Invalid phone number format");
    }
}