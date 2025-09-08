using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Email;
using Streetcode.BLL.MediatR.Email;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Email;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Email
{
    public class SendEmailCommandValidatorTests
    {
        private readonly SendEmailCommandValidator _validator;

        public SendEmailCommandValidatorTests()
        {
            _validator = new SendEmailCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Null()
        {
            // Arrange
            var command = new SendEmailCommand(null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email)
                  .WithErrorMessage(Errors_Validation.IsRequired.FormatWith("Email"));
        }

        [Fact]
        public void Should_Have_Error_When_EmailDTO_From_Is_Invalid()
        {
            // Arrange
            var emailDto = new EmailDTO
            {
                From = "invalid-email",
                Content = "Test content"
            };
            var command = new SendEmailCommand(emailDto);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email.From)
                  .WithErrorMessage(Errors_Validation.EmailAddressFormat);
        }

        [Fact]
        public void Should_Have_Error_When_EmailDTO_Content_Is_Empty()
        {
            // Arrange
            var emailDto = new EmailDTO
            {
                From = "test@test.com",
                Content = ""
            };
            var command = new SendEmailCommand(emailDto);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email.Content)
                  .WithErrorMessage(Errors_Validation.CannotBeEmpty.FormatWith("Content"));
        }

        [Fact]
        public void Should_Not_Have_Error_When_EmailDTO_Is_Valid()
        {
            // Arrange
            var emailDto = new EmailDTO
            {
                From = "test@test.com",
                Content = "Valid email content"
            };
            var command = new SendEmailCommand(emailDto);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
