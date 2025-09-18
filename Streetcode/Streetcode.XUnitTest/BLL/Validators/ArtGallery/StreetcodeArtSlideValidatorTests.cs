using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Validators.ArtGallery;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.ArtGallery
{
    public class StreetcodeArtSlideValidatorTests
    {
        private readonly StreetcodeArtSlideValidator _validator;

        public StreetcodeArtSlideValidatorTests()
        {
            _validator = new StreetcodeArtSlideValidator();
        }

        [Fact]
        public void ShouldReturnError_WhenTemplateIsInvalid()
        {
            // Arrange
            var dto = CreateDto(d => d.Template = (GallerySlideTemplate)999);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Template);
        }

        [Fact]
        public void ShouldNotReturnError_WhenTemplateIsValid()
        {
            // Arrange
            var dto = CreateDto();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Template);
        }

        [Fact]
        public void ShouldNotReturnError_WhenStreetcodeArtsHasValidElement()
        {
            // Arrange
            var dto = CreateDto(d => d.StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
            {
                new StreetcodeArtCreateUpdateDTO { Index = 1, ArtId = 2 }
            });

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ShouldReturnError_WhenStreetcodeArtsIsEmpty()
        {
            // Arrange
            var dto = CreateDto();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            Assert.False(result.IsValid);
        }

        private static StreetcodeArtSlideCreateUpdateDTO CreateDto(Action<StreetcodeArtSlideCreateUpdateDTO>? customize = null)
        {
            var dto = new StreetcodeArtSlideCreateUpdateDTO
            {
                Template = GallerySlideTemplate.OneToTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>()
            };

            customize?.Invoke(dto);
            return dto;
        }
    }
}
