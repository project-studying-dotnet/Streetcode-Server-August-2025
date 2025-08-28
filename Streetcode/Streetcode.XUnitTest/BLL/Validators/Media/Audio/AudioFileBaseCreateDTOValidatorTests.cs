using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Validators.Media.Audio;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Media.Audio
{
    public class AudioFileBaseCreateDTOValidatorTests
    {
        private readonly AudioFileBaseCreateDTOValidator _validator;

        public AudioFileBaseCreateDTOValidatorTests()
        {
            _validator = new AudioFileBaseCreateDTOValidator();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Null_Or_Empty()
        {
            var model = new AudioFileBaseCreateDTO { Description = null };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);

            model.Description = "";
            result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Valid_Length()
        {
            var model = new AudioFileBaseCreateDTO { Description = new string('a', 500) };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Max_Length()
        {
            var model = new AudioFileBaseCreateDTO { Description = new string('a', 501) };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description cannot exceed 500 characters.");
        }
    }
}
