using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.AdditionalContent.Tag;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.AdditionalContent.Tag;

public class TagValidatorTests
{
    private readonly TagValidator _validator;

    public TagValidatorTests()
    {
        _validator = new TagValidator();
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenAllFieldsAreValid()
    {
        // Arrange
        var tag = GetValidTag();

        // Act
        var result = _validator.Validate(tag);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ShouldReturnError_WhenTitleIsEmpty()
    {
        // Arrange
        var tag = GetValidTag();
        tag.Title = string.Empty;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("Title");

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldHaveValidationErrorFor(t => t.Title)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTitleIsTooLong()
    {
        // Arrange
        var tag = GetValidTag();
        tag.Title = new string('A', TagValidator.TitleMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Title", TagValidator.TitleMaxLength);

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldHaveValidationErrorFor(t => t.Title)
            .WithErrorMessage(expectedMessage);
    }

    private static CreateTagDTO GetValidTag()
    {
        return new CreateTagDTO()
        {
            Title = "Test",
        };
    }
}
