using FluentValidation;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.BLL.Validators.Media.Image;
using Streetcode.BLL.DTO.Media.Images;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Media.Image
{
    public class CreateImageCommandValidatorTests
    {
        private readonly CreateImageCommandValidator _validator;

        public CreateImageCommandValidatorTests()
        {
            _validator = new CreateImageCommandValidator();
        }

        [Fact]
        public void Validate_Should_Return_Error_When_Image_Is_Null()
        {
            // Arrange
            var command = new CreateImageCommand(null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Image");
        }

        [Fact]
        public void Validate_Should_Be_Valid_When_Image_Is_Valid()
        {
            // Arrange
            var imageDto = new ImageFileBaseCreateDTO
            {
                Title = "Test Image",
                MimeType = "image/png",
                Extension = "png"
            };
            var command = new CreateImageCommand(imageDto);

            // Act
            var result = _validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
