using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Validators.News;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.News
{
    public class NewsDTOValidatorTests
    {
        private readonly NewsDTOValidator _validator;

        public NewsDTOValidatorTests()
        {
            _validator = new NewsDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = new NewsDTO { Title = "", Text = "Content", URL = "https://valid.url", CreationDate = DateTime.Now };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Too_Short()
        {
            var dto = new NewsDTO { Title = "A", Text = "Content", URL = "https://valid.url", CreationDate = DateTime.Now };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Text_Is_Empty()
        {
            var dto = new NewsDTO { Title = "Title", Text = "", URL = "https://valid.url", CreationDate = DateTime.Now };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Text);
        }

        [Fact]
        public void Should_Have_Error_When_URL_Is_Invalid()
        {
            var dto = new NewsDTO { Title = "Title", Text = "Content", URL = "invalid-url", CreationDate = DateTime.Now };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.URL);
        }

        [Fact]
        public void Should_Have_Error_When_ImageId_Less_Than_1()
        {
            var dto = new NewsDTO { Title = "Title", Text = "Content", URL = "https://valid.url", CreationDate = DateTime.Now, ImageId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ImageId);
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Valid()
        {
            var dto = new NewsDTO
            {
                Title = "Valid Title",
                Text = "Valid content",
                URL = "https://valid.url",
                CreationDate = DateTime.Now,
                ImageId = 5
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
            result.ShouldNotHaveValidationErrorFor(x => x.Text);
            result.ShouldNotHaveValidationErrorFor(x => x.URL);
            result.ShouldNotHaveValidationErrorFor(x => x.CreationDate);
            result.ShouldNotHaveValidationErrorFor(x => x.ImageId);
        }
    }
}
