using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Enums;
using Streetcode.BLL.Validators.Streetcode.Toponyms;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Streetcode.Toponyms;

public class StreetcodeToponymValidatorTests
{
    private readonly StreetcodeToponymValidator _streetcodeToponymValidator;

    public StreetcodeToponymValidatorTests()
    {
        _streetcodeToponymValidator = new StreetcodeToponymValidator();
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenTimelineIsValid()
    {
        // Arrange
        var toponym = GetValidToponym();

        // Act
        var res = _streetcodeToponymValidator.TestValidate(toponym);

        // Assert
        Assert.True(res.IsValid);
    }

    [Fact]
    public void ShouldReturnValidationError_WhenStreetNameExceedsMaxLength()
    {
        // Arrange
        var toponym = GetValidToponym();
        toponym.StreetName = new string('e', StreetcodeToponymValidator.StreetNameMaxLength + 1);
        var erorMessage = "MaxLength";

        // Act
        var res = _streetcodeToponymValidator.TestValidate(toponym);

        // Assert
        res.ShouldHaveValidationErrorFor(x => x.StreetName).WithErrorMessage(erorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public void ShouldReturnValidationError_WhenStreetNameIsEmpty(string invalidTitle)
    {
        // Arrange
        var toponym = GetValidToponym();
        toponym.StreetName = invalidTitle;
        var erorMessage = "CannotBeEmpty";

        // Act
        var res = _streetcodeToponymValidator.TestValidate(toponym);

        // Assert
        res.ShouldHaveValidationErrorFor(x => x.StreetName).WithErrorMessage(erorMessage);
    }

    [Fact]
    public void ShouldReturnValidationError_WhenModelStateIsNotInEnum()
    {
        // Arrange
        var toponym = GetValidToponym();
        toponym.ModelState = (ModelState)12;
        var erorMessage = "Invalid";

        // Act
        var res = _streetcodeToponymValidator.TestValidate(toponym);

        // Assert
        res.ShouldHaveValidationErrorFor(x => x.ModelState).WithErrorMessage(erorMessage);
    }

    private static StreetcodeToponymCreateUpdateDTO GetValidToponym()
    {
        return new StreetcodeToponymCreateUpdateDTO()
        {
            StreetcodeId = 1,
            StreetName = "Lesia Ukrainka",
            ToponymId = 1,
            ModelState = ModelState.Deleted,
        };
    }
}
