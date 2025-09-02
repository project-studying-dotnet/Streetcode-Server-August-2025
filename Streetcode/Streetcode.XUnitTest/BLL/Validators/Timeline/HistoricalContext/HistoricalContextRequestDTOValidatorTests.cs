using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Validators.Timeline.HistoricalContext;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Timeline.HistoricalContext
{
    public class HistoricalContextRequestDtoValidatorTests
    {
        private readonly HistoricalContextRequestDtoValidator _validator;

        public HistoricalContextRequestDtoValidatorTests()
        {
            _validator = new HistoricalContextRequestDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Id_And_Title_Null()
        {
            const string errorMessage = "Context must have either an ID or a title.";

            var context = new HistoricalContextRequestDto
            {
                Id = null,
                Title = null
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Id_And_Title_Provided()
        {
            const string errorMessage = "Cannot provide both an ID and a title for one context.";

            var context = new HistoricalContextRequestDto
            {
                Id = 1,
                Title = "Valid Title"
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Id_Null_And_Title_Empty()
        {
            const string errorMessage = "Context must have either an ID or a title.";

            var context = new HistoricalContextRequestDto
            {
                Id = null,
                Title = "   "
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(errorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Have_Error_When_Id_Less_Than_One(int invalidId)
        {
            const string errorMessage = "ID must be greater than zero.";

            var context = new HistoricalContextRequestDto
            {
                Id = invalidId,
                Title = null
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Too_Long()
        {
            const string errorMessage = "Title cannot exceed 50 characters.";
            var context = new HistoricalContextRequestDto
            {
                Id = null,
                Title = new string('a', 51)
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Has_Invalid_Characters()
        {
            const string errorMessage = "Title can only contain letters and spaces.";

            var context = new HistoricalContextRequestDto
            {
                Id = null,
                Title = "Invalid@Title#123"
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Valid_Id()
        {
            var context = new HistoricalContextRequestDto
            {
                Id = 1,
                Title = null
            };

            var result = _validator.TestValidate(context);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(25)]
        [InlineData(49)]
        [InlineData(50)]
        public void Should_Not_Have_Error_When_Valid_Title(int titleLength)
        {
            var context = new HistoricalContextRequestDto
            {
                Id = null,
                Title = new string('a', titleLength)
            };

            var result = _validator.TestValidate(context);

            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }
    }
}
