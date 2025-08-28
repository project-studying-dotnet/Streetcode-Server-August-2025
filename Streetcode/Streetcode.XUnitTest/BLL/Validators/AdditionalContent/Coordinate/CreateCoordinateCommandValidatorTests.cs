using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.BLL.Validators.AdditionalContent.Coordinate;
using Xunit;
namespace Streetcode.XUnitTest.BLL.Validators.AdditionalContent.Coordinate
{
    public class CreateCoordinateCommandValidatorTests
    {
        private readonly CreateCoordinateCommandValidator _validator;

        public CreateCoordinateCommandValidatorTests()
        {
            _validator = new CreateCoordinateCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_StreetcodeCoordinate_Is_Null()
        {
            // Arrange
            var command = new CreateCoordinateCommand(null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.StreetcodeCoordinate)
                  .WithErrorMessage("Coordinate data is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_StreetcodeCoordinate_Is_Not_Null()
        {
            // Arrange
            var dto = new StreetcodeCoordinateDTO
            {
                Latitude = 50
            };

            var command = new CreateCoordinateCommand(dto);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.StreetcodeCoordinate);
        }
    }
}
