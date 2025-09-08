using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.AdditionalContent.Coordinate;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.AdditionalContent.Coordinate
{
    public class UpdateCoordinateCommandValidatorTests
    {
        private readonly UpdateCoordinateCommandValidator _validator;

        public UpdateCoordinateCommandValidatorTests()
        {
            _validator = new UpdateCoordinateCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_StreetcodeCoordinate_Is_Null()
        {
            var command = new UpdateCoordinateCommand(null);

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeCoordinate)
                  .WithErrorMessage(Errors_Validation.IsRequiredData.FormatWith("StreetcodeCoordinate"));
        }

        [Fact]
        public void Should_Have_Error_When_StreetcodeCoordinate_Invalid()
        {
            var invalidDto = new StreetcodeCoordinateDTO
            {
                Id = 0,
                Latitude = 100m,
                Longtitude = -200m,
                StreetcodeId = 0
            };

            var command = new UpdateCoordinateCommand(invalidDto);

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeCoordinate.Id);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeCoordinate.Latitude);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeCoordinate.Longtitude);
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeCoordinate.StreetcodeId);
        }

        [Fact]
        public void Should_Not_Have_Error_When_StreetcodeCoordinate_Valid()
        {
            var validDto = new StreetcodeCoordinateDTO
            {
                Id = 1,
                Latitude = 45m,
                Longtitude = 90m,
                StreetcodeId = 10
            };

            var command = new UpdateCoordinateCommand(validDto);

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
