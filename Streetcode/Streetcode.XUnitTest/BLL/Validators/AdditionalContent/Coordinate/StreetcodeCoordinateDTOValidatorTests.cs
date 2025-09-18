using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.AdditionalContent.Coordinate;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.AdditionalContent.Coordinate
{
    public class StreetcodeCoordinateDTOValidatorTests
    {
        private readonly StreetcodeCoordinateDTOValidator _validator;

        public StreetcodeCoordinateDTOValidatorTests()
        {
            _validator = new StreetcodeCoordinateDTOValidator();
        }

        [Theory]
        [InlineData(-91)]
        [InlineData(91)]
        public void Should_Have_Error_When_Latitude_Out_Of_Range(double latitude)
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Latitude = (decimal)latitude,
                Longtitude = 0,
                StreetcodeId = 1,
                Id = 0
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
                Latitude = 0,
                Longtitude = (decimal)longitude,
                StreetcodeId = 1,
                Id = 0
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
                Latitude = 0,
                Longtitude = 0,
                StreetcodeId = streetcodeId,
                Id = 0
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeId)
                  .WithErrorMessage(Errors_Validation.GreaterThan.FormatWith("StreetcodeId", 0));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void Should_Have_Error_When_Id_Is_Set(int id)
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Latitude = 0,
                Longtitude = 0,
                StreetcodeId = 1,
                Id = id
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id must not be set when creating a new coordinate.");
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_DTO()
        {
            var dto = new StreetcodeCoordinateDTO
            {
                Latitude = 50,
                Longtitude = 30,
                StreetcodeId = 1,
                Id = 0
            };

            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
