using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Partners.Create;
using Streetcode.BLL.Validators.Partners;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Partners
{
    public class CreatePartnerSourceLinkDTOValidatorTests
    {
        private readonly CreatePartnerSourceLinkDTOValidator _validator;

        public CreatePartnerSourceLinkDTOValidatorTests()
        {
            _validator = new CreatePartnerSourceLinkDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Id_Not_Zero()
        {
            var dto = new CreatePartnerSourceLinkDTO { Id = 5, LogoType = default, TargetUrl = "https://valid.url" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Have_Error_When_TargetUrl_Invalid()
        {
            var dto = new CreatePartnerSourceLinkDTO { Id = 0, LogoType = default, TargetUrl = "invalid-url" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.TargetUrl);
        }

        [Fact]
        public void Should_Have_Error_When_TargetUrl_Empty()
        {
            var dto = new CreatePartnerSourceLinkDTO { Id = 0, LogoType = default, TargetUrl = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.TargetUrl);
        }
    }
}
