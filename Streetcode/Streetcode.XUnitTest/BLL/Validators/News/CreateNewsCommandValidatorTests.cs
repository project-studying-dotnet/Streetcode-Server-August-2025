using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.Validators.News;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.News
{
    public class CreateNewsCommandValidatorTests
    {
        private readonly CreateNewsCommandValidator _validator;

        public CreateNewsCommandValidatorTests()
        {
            _validator = new CreateNewsCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_NewNews_Is_Null()
        {
            // Arrange
            var command = new CreateNewsCommand(null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.NewNews)
                  .WithErrorMessage("News data is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_NewNews_Is_Valid()
        {
            // Arrange
            var newsDto = new NewsDTO
            {
                Title = "Test News",
                ImageId = 1
            };

            var command = new CreateNewsCommand(newsDto);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.NewNews);
        }
    }
}
