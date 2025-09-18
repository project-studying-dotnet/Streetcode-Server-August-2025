using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Validators.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Partners
{
    public class StreetcodeShortDTOValidatorTests
    {
        private readonly StreetcodeShortDTOValidator _validator;

        public StreetcodeShortDTOValidatorTests()
        {
            _validator = new StreetcodeShortDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Id_Less_Than_Or_Equal_Zero()
        {
            var dto = new StreetcodeShortDTO { Id = 0, Title = "Valid Title" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Empty()
        {
            var dto = new StreetcodeShortDTO { Id = 1, Title = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Valid()
        {
            var dto = new StreetcodeShortDTO { Id = 1, Title = "Valid Title" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }
    }
}
