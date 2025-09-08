using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Media.Audio;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Media.Audio
{
    public class CreateAudioCommandValidatorTests
    {
        private readonly CreateAudioCommandValidator _validator;

        public CreateAudioCommandValidatorTests()
        {
            _validator = new CreateAudioCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Audio_Is_Null()
        {
            // Arrange
            var command = new CreateAudioCommand(null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Audio)
                  .WithErrorMessage(Errors_Validation.IsRequired.FormatWith("Audio"));
        }

        [Fact]
        public void Should_Have_Error_When_Description_Too_Long()
        {
            // Arrange
            var audioDto = new AudioFileBaseCreateDTO
            {
                Description = new string('a', 501)
            };
            var command = new CreateAudioCommand(audioDto);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Audio.Description)
                  .WithErrorMessage(Errors_Validation.MaxLength.FormatWith("Description", 500));
        }
    }
}
