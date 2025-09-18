using FluentValidation;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Validators.Media.Image;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Media.Image
{
    public class ImageFileBaseCreateDTOValidatorTests
    {
        private readonly ImageFileBaseCreateDTOValidator _validator;

        public ImageFileBaseCreateDTOValidatorTests()
        {
            _validator = new ImageFileBaseCreateDTOValidator();
        }

        [Fact]
        public void Validate_Should_Return_Error_When_Alt_Too_Long()
        {
            // Arrange
            var dto = new ImageFileBaseCreateDTO
            {
                Alt = new string('a', 201),
                Title = "Test Image",
                MimeType = "image/png",
                Extension = "png"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Alt");
        }

        [Fact]
        public void Validate_Should_Be_Valid_When_Alt_Is_Correct()
        {
            // Arrange
            var dto = new ImageFileBaseCreateDTO
            {
                Alt = "Valid alt text",
                Title = "Test Image",
                MimeType = "image/png",
                Extension = "png"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
