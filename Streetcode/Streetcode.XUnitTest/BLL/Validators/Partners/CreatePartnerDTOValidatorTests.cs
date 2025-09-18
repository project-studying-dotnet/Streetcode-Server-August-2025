using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Validators.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Partners
{
    public class CreatePartnerDTOValidatorTests
    {
        private readonly CreatePartnerDTOValidator _validator;

        public CreatePartnerDTOValidatorTests()
        {
            _validator = new CreatePartnerDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Id_Is_Not_Zero()
        {
            var dto = new CreatePartnerDTO { Id = 5 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = new CreatePartnerDTO { Id = 0, Title = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_TargetUrl_Is_Invalid()
        {
            var dto = new CreatePartnerDTO { Id = 0, Title = "Test", LogoId = 1, TargetUrl = "invalid-url" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.TargetUrl);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Too_Long()
        {
            var longDescription = new string('a', 601);
            var dto = new CreatePartnerDTO { Id = 0, Title = "Test", LogoId = 1, Description = longDescription };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}
