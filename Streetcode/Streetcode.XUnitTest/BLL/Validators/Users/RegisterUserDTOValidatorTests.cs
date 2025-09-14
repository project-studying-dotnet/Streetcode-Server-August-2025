using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Validators.Users;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Users;

public class RegisterUserDTOValidatorTests
{
    private readonly RegisterUserDTOValidator _validator;

    public RegisterUserDTOValidatorTests()
    {
        _validator = new RegisterUserDTOValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null_Or_Empty()
    {
        var model = new RegisterUserDTO { Email = null };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Format_Is_Invalid()
    {
        var model = new RegisterUserDTO { Email = "invalidemail" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Exceeds_MaxLength()
    {
        var model = new RegisterUserDTO { Email = new string('a', 51) + "@mail.com" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var model = new RegisterUserDTO { Password = "A1b" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Uppercase()
    {
        var model = new RegisterUserDTO { Password = "lowercase1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Lowercase()
    {
        var model = new RegisterUserDTO { Password = "UPPERCASE1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Digit()
    {
        var model = new RegisterUserDTO { Password = "Password" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Exceeds_MaxLength()
    {
        var model = new RegisterUserDTO { Password = new string('A', 21) + "1a" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Exceeds_MaxLength()
    {
        var model = new RegisterUserDTO { UserName = new string('a', 21) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var model = new RegisterUserDTO { Name = new string('a', 51) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Surname_Exceeds_MaxLength()
    {
        var model = new RegisterUserDTO { Surname = new string('a', 51) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Surname);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Format_Is_Invalid()
    {
        var model = new RegisterUserDTO { PhoneNumber = "123ABC" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        var model = new RegisterUserDTO
        {
            Email = "test@mail.com",
            Password = "StrongP@ss1",
            UserName = "testuser",
            Name = "Denys",
            Surname = "Pavski",
            PhoneNumber = "+380991112233"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}