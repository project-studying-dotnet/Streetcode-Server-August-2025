using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Validators.Auth;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator;

    public LoginValidatorTests()
    {
        _validator = new LoginValidator();
    }

    [Fact]
    public void Validate_ValidEmailAndPassword_ShouldBeValid()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "test@example.com",
            Password = "validPassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyLogin_ShouldHaveError()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = string.Empty,
            Password = "validPassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "invalid-email",
            Password = "validPassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }

    [Fact]
    public void Validate_NullLogin_ShouldHaveError()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = null!,
            Password = "validPassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveError()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "test@example.com",
            Password = string.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_NullPassword_ShouldHaveError()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "test@example.com",
            Password = null!
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_InvalidEmailAndEmptyPassword_ShouldHaveTwoErrors()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "invalid-email",
            Password = string.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Login);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ValidEmailWithSpecialChars_ShouldBeValid()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "user+test_%-@example.com",
            Password = "validPassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmailWithDoubleDot_ShouldHaveError()
    {
        // Arrange
        var dto = new UserLoginDTO
        {
            Login = "user..test@example.com",
            Password = "validPassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
}
