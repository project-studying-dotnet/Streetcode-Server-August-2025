using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.MediatR.Users.Register;
using Streetcode.BLL.Validators.Users;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Users;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_RegisteredUserDto_Is_Null()
    {
        var command = new RegisterUserCommand(null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.registeredUserDto);
    }

    [Fact]
    public void Should_Pass_When_RegisteredUserDto_Is_Valid()
    {
        var dto = new RegisterUserDTO
        {
            Email = "test@mail.com",
            Password = "StrongP@ss1",
            UserName = "testuser",
            Name = "Denys",
            Surname = "Pavski",
            PhoneNumber = "+380991112233"
        };

        var command = new RegisterUserCommand(dto);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.registeredUserDto);
    }

    [Fact]
    public void Should_Have_Errors_When_RegisteredUserDto_Is_Invalid()
    {
        var dto = new RegisterUserDTO
        {
            Email = "invalidemail",
            Password = "123",
            UserName = new string('a', 25)
        };

        var command = new RegisterUserCommand(dto);
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.registeredUserDto.Email);
        result.ShouldHaveValidationErrorFor(x => x.registeredUserDto.Password);
        result.ShouldHaveValidationErrorFor(x => x.registeredUserDto.UserName);
    }
}