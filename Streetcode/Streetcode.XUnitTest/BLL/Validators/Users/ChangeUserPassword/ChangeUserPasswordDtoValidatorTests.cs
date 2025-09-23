using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Users.ChangePassword;
using Streetcode.BLL.Validators.Users.ChangeUserPasswordValidator;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Users.ChangeUserPassword
{
    public class ChangeUserPasswordDtoValidatorTests
    {
        private readonly ChangeUserPasswordDtoValidator _validator;

        public ChangeUserPasswordDtoValidatorTests()
        {
            _validator = new ChangeUserPasswordDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Null()
        {
            var model = new ChangePasswordRequestDto { NewPassword = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var model = new ChangePasswordRequestDto { NewPassword = "" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            var model = new ChangePasswordRequestDto { NewPassword = "abc12" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Long()
        {
            var model = new ChangePasswordRequestDto { NewPassword = new string('A', 21) + "1a" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Have_Error_When_Missing_Uppercase()
        {
            var model = new ChangePasswordRequestDto { NewPassword = "lowercase1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Have_Error_When_Missing_Lowercase()
        {
            var model = new ChangePasswordRequestDto { NewPassword = "UPPERCASE1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Have_Error_When_Missing_Digit()
        {
            var model = new ChangePasswordRequestDto { NewPassword = "PasswordOnly" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void Should_Pass_When_Password_Is_Valid()
        {
            var model = new ChangePasswordRequestDto { NewPassword = "Valid123" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
