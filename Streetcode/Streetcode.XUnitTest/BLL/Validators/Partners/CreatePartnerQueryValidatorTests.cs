using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.BLL.Validators.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Partners
{
    public class CreatePartnerQueryValidatorTests
    {
        private readonly CreatePartnerQueryValidator _validator;

        public CreatePartnerQueryValidatorTests()
        {
            _validator = new CreatePartnerQueryValidator();
        }

        [Fact]
        public void Should_Have_Error_When_newPartner_Is_Null()
        {
            var query = new CreatePartnerQuery(null);
            var result = _validator.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.newPartner);
        }

        [Fact]
        public void Should_Have_Error_When_newPartner_Invalid()
        {
            var dto = new CreatePartnerDTO { Id = 5, Title = "", LogoId = 0 };
            var query = new CreatePartnerQuery(dto);

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.newPartner.Id);
            result.ShouldHaveValidationErrorFor(x => x.newPartner.Title);
            result.ShouldHaveValidationErrorFor(x => x.newPartner.LogoId);
        }

        [Fact]
        public void Should_Not_Have_Error_When_newPartner_Valid()
        {
            var dto = new CreatePartnerDTO
            {
                Id = 0,
                Title = "Valid Partner",
                LogoId = 1,
                TargetUrl = "https://valid.url",
                UrlTitle = "Url Title",
                Description = "Some description"
            };
            var query = new CreatePartnerQuery(dto);

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.newPartner);
        }
    }
}
