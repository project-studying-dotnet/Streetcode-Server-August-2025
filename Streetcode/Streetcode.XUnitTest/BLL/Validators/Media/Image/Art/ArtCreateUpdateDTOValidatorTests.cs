using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Enums;
using Streetcode.BLL.Validators.Media.Image.Art;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Media.Image.Art
{
    public class ArtCreateUpdateDTOValidatorTests
    {
        private readonly ArtCreateUpdateDTOValidator _validator;

        public ArtCreateUpdateDTOValidatorTests()
        {
            _validator = new ArtCreateUpdateDTOValidator();
        }

        [Fact]
        public void ShouldReturnError_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var dto = CreateDto(d => d.Title = new string('a', ArtCreateUpdateDTOValidator.MaxTitleLength + 1));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage($"Title cannot exceed {ArtCreateUpdateDTOValidator.MaxTitleLength} characters.");
        }

        [Fact]
        public void ShouldNotReturnError_WhenTitleIsValidLength()
        {
            // Arrange
            var dto = CreateDto(d => d.Title = new string('a', ArtCreateUpdateDTOValidator.MaxTitleLength));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void ShouldReturnError_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var dto = CreateDto(d => d.Description = new string('a', ArtCreateUpdateDTOValidator.MaxDescriptionLength + 1));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage($"Description cannot exceed {ArtCreateUpdateDTOValidator.MaxDescriptionLength} characters.");
        }

        [Fact]
        public void ShouldNotReturnError_WhenDescriptionIsValidLength()
        {
            // Arrange
            var dto = CreateDto(d => d.Description = new string('a', ArtCreateUpdateDTOValidator.MaxDescriptionLength));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void ShouldReturnError_WhenModelStateInvalid()
        {
            // Arrange
            var dto = CreateDto(d => d.ModelState = (ModelState)999);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ModelState)
                  .WithErrorMessage("Invalid ModelState value.");
        }

        [Fact]
        public void ShouldNotReturnError_WhenModelStateValid()
        {
            // Arrange
            var dto = CreateDto(d => d.ModelState = ModelState.Updated);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ModelState);
        }

        private static ArtCreateUpdateDTO CreateDto(Action<ArtCreateUpdateDTO>? customize = null)
        {
            var dto = new ArtCreateUpdateDTO
            {
                Title = "title",
                Description = "desc",
                ImageId = 1,
                ModelState = ModelState.Created
            };

            customize?.Invoke(dto);
            return dto;
        }
    }
}
