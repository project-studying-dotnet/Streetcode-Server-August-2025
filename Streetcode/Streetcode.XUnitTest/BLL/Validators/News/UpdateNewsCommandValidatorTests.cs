using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.BLL.Validators.News;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.News
{
    public class UpdateNewsCommandValidatorTests
    {
        private readonly UpdateNewsCommandValidator _validator;

        public UpdateNewsCommandValidatorTests()
        {
            _validator = new UpdateNewsCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_News_Is_Null()
        {
            var command = new UpdateNewsCommand(null);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.news);
        }

        [Fact]
        public void Should_Have_Error_When_NewsDTO_Is_Invalid()
        {
            var dto = new NewsDTO { Id = 1, Title = "", Text = "", URL = "invalid-url", CreationDate = default };
            var command = new UpdateNewsCommand(dto);

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.news.Title);
            result.ShouldHaveValidationErrorFor(x => x.news.Text);
            result.ShouldHaveValidationErrorFor(x => x.news.URL);
            result.ShouldHaveValidationErrorFor(x => x.news.CreationDate);
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Valid()
        {
            var dto = new NewsDTO
            {
                Id = 1,
                Title = "Valid Title",
                Text = "Valid content",
                URL = "https://valid.url",
                CreationDate = System.DateTime.Now
            };
            var command = new UpdateNewsCommand(dto);

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.news);
            result.ShouldNotHaveValidationErrorFor(x => x.news.Id);
            result.ShouldNotHaveValidationErrorFor(x => x.news.Title);
            result.ShouldNotHaveValidationErrorFor(x => x.news.Text);
            result.ShouldNotHaveValidationErrorFor(x => x.news.URL);
            result.ShouldNotHaveValidationErrorFor(x => x.news.CreationDate);
        }
    }
}
