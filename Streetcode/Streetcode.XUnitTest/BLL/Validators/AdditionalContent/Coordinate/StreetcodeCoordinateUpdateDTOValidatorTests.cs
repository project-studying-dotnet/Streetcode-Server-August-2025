using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.AdditionalContent.Coordinate;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.AdditionalContent.Coordinate
{
    public class StreetcodeCoordinateUpdateDTOValidatorTests
    {
        private readonly StreetcodeCoordinateUpdateDTOValidator _validator;

        public StreetcodeCoordinateUpdateDTOValidatorTests()
        {
            _validator = new StreetcodeCoordinateUpdateDTOValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Have_Error_When_Id_Invalid(int id)
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Id = id,
                Latitude = 0m,
                Longtitude = 0m,
                StreetcodeId = 1
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage(Errors_Validation.GreaterThan.FormatWith("Id", 0));
        }

        [Theory]
        [InlineData(-91)]
        [InlineData(91)]
        public void Should_Have_Error_When_Latitude_Out_Of_Range(double latitude)
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Id = 1,
                Latitude = (decimal)latitude,
                Longtitude = 0m,
                StreetcodeId = 1
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Latitude)
                  .WithErrorMessage(Errors_Validation.MustBeBetween.FormatWith("Latitude", -90, 90));
        }

        [Theory]
        [InlineData(-181)]
        [InlineData(181)]
        public void Should_Have_Error_When_Longitude_Out_Of_Range(double longitude)
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Id = 1,
                Latitude = 0m,
                Longtitude = (decimal)longitude,
                StreetcodeId = 1
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Longtitude)
                  .WithErrorMessage(Errors_Validation.MustBeBetween.FormatWith("Longtitude", -180, 180));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Should_Have_Error_When_StreetcodeId_Invalid(int streetcodeId)
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Id = 1,
                Latitude = 0m,
                Longtitude = 0m,
                StreetcodeId = streetcodeId
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeId)
                  .WithErrorMessage(Errors_Validation.GreaterThan.FormatWith("StreetcodeId", 0));
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Valid()
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Id = 1,
                Latitude = 45m,
                Longtitude = 90m,
                StreetcodeId = 10
            };

            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
