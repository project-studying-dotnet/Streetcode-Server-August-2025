using FluentValidation.TestHelper;
using Streetcode.BLL.MediatR.Team.Create;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Validators.Team.Position;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Team.Position
{
    public class CreatePositionQueryValidatorTests
    {
        private readonly CreatePositionQueryValidator _validator;

        public CreatePositionQueryValidatorTests()
        {
            _validator = new CreatePositionQueryValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Position_Name_Is_Empty()
        {
            var positionDto = new PositionDTO { Position = "" };
            var query = new CreatePositionQuery(positionDto);
            var result = _validator.TestValidate(query);
            result.ShouldHaveValidationErrorFor(q => q.position.Position);
        }

        [Fact]
        public void Should_Have_Error_When_Position_Name_Too_Long()
        {
            var longName = new string('a', 51);
            var positionDto = new PositionDTO { Position = longName };
            var query = new CreatePositionQuery(positionDto);
            var result = _validator.TestValidate(query);
            result.ShouldHaveValidationErrorFor(q => q.position.Position);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Position()
        {
            var positionDto = new PositionDTO { Position = "Developer" };
            var query = new CreatePositionQuery(positionDto);
            var result = _validator.TestValidate(query);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
