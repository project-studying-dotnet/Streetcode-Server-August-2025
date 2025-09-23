using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Users.ChangePassword;
using Streetcode.BLL.MediatR.Users.ChangePassword;
using Streetcode.BLL.Validators.Users.ChangeUserPasswordCommandValidator;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Users.ChangeUserPassword
{
    public class ChangeUserPasswordCommandValidatorTests
    {
        private readonly ChangeUserPasswordCommandValidator _validator;

        public ChangeUserPasswordCommandValidatorTests()
        {
            _validator = new ChangeUserPasswordCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_ChangePasswordRequestDto_Is_Null()
        {
            var command = new ChangePasswordCommand(null);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.changePasswordRequestDto);
        }

        [Fact]
        public void Should_Pass_When_ChangePasswordRequestDto_Is_Valid()
        {
            var dto = new ChangePasswordRequestDto { NewPassword = "StrongPass1" };
            var command = new ChangePasswordCommand(dto);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.changePasswordRequestDto);
        }

        [Fact]
        public void Should_Have_Error_When_ChangePasswordRequestDto_Is_Invalid()
        {
            var dto = new ChangePasswordRequestDto { NewPassword = "abc" }; // короткий пароль
            var command = new ChangePasswordCommand(dto);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.changePasswordRequestDto.NewPassword);
        }
    }
}
