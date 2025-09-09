using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
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
            string errorMessage = Errors_Timeline.Context_MustHaveIdOrTitle;

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
            string errorMessage = Errors_Timeline.Context_CannotHaveBothIdAndTitle;

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
            string errorMessage = Errors_Timeline.Context_MustHaveIdOrTitle;

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
            string errorMessage = Errors_Validation.GreaterThan.FormatWith("Id", 0);

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
            string errorMessage = Errors_Validation.MaxLength.FormatWith("Title", HistoricalContextRequestDtoValidator.MaxTitleLength);
            var context = new HistoricalContextRequestDto
            {
                Id = null,
                Title = new string('a', HistoricalContextRequestDtoValidator.MaxTitleLength + 1)
            };

            var result = _validator.TestValidate(context);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(errorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Has_Invalid_Characters()
        {
            string errorMessage = Errors_Validation.InvalidCharacters.FormatWith("Title");

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
