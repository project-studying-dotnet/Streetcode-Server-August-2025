using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Email;
using Streetcode.BLL.Validators.Email;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Email
{
    public class EmailDTOValidatorTests
    {
        private readonly EmailDTOValidator _validator;

        public EmailDTOValidatorTests()
        {
            _validator = new EmailDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_From_Invalid_Email()
        {
            var email = new EmailDTO
            {
                From = "invalid-email",
                Content = "Valid content"
            };

            var result = _validator.TestValidate(email);
            result.ShouldHaveValidationErrorFor(x => x.From)
                  .WithErrorMessage("Sender must be a valid email address.");
        }

        [Fact]
        public void Should_Have_Error_When_From_Too_Long()
        {
            var email = new EmailDTO
            {
                From = new string('a', 81) + "@test.com",
                Content = "Valid content"
            };

            var result = _validator.TestValidate(email);
            result.ShouldHaveValidationErrorFor(x => x.From)
                  .WithErrorMessage("Sender address cannot exceed 80 characters.");
        }

        [Fact]
        public void Should_Have_Error_When_Content_Empty()
        {
            var email = new EmailDTO
            {
                From = "test@test.com",
                Content = ""
            };

            var result = _validator.TestValidate(email);
            result.ShouldHaveValidationErrorFor(x => x.Content)
                  .WithErrorMessage("Email content is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Content_Too_Long()
        {
            var email = new EmailDTO
            {
                From = "test@test.com",
                Content = new string('a', 501)
            };

            var result = _validator.TestValidate(email);
            result.ShouldHaveValidationErrorFor(x => x.Content)
                  .WithErrorMessage("Email content cannot exceed 500 characters.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Email_Valid()
        {
            var email = new EmailDTO
            {
                From = "test@test.com",
                Content = "Valid content"
            };

            var result = _validator.TestValidate(email);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}