using Streetcode.BLL.DTO.Users;
using FluentValidation.TestHelper;
using Streetcode.BLL.MediatR.Auth.Login;
using Streetcode.BLL.Validators.Auth;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        var loginValidator = new LoginValidator();
        _validator = new LoginCommandValidator(loginValidator);
    }

    [Fact]
    public void Validate_ValidLoginCommand_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand(new UserLoginDTO
        {
            Login = "test@example.com",
            Password = "ValidPassword123"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldFailWithFormatError()
    {
        // Arrange
        var command = new LoginCommand(new UserLoginDTO
        {
            Login = "invalid-email",
            Password = "ValidPassword123"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.userLoginDTO.Login);
    }

    [Fact]
    public void Validate_EmptyLogin_ShouldFailWithEmptyError()
    {
        // Arrange
        var command = new LoginCommand(new UserLoginDTO
        {
            Login = string.Empty,
            Password = "ValidPassword123"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.userLoginDTO.Login);
    }

    [Fact]
    public void Validate_NullLogin_ShouldFailWithEmptyError()
    {
        // Arrange
        var command = new LoginCommand(new UserLoginDTO
        {
            Login = null,
            Password = "ValidPassword123"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.userLoginDTO.Login);
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFailWithEmptyError()
    {
        // Arrange
        var command = new LoginCommand(new UserLoginDTO
        {
            Login = "test@example.com",
            Password = string.Empty
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.userLoginDTO.Password);
    }

    [Fact]
    public void Validate_ComplexValidEmail_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand(new UserLoginDTO
        {
            Login = "user.name+test@subdomain.example.co.uk",
            Password = "ComplexPass123!"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
