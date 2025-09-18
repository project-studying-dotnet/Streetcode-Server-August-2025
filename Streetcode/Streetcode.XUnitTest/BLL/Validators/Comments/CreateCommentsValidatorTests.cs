using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Comments;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Comments;

public class CreateCommentsValidatorTests
{
    private readonly CreateCommentCommandValidator _validator;

    public CreateCommentsValidatorTests()
    {
        _validator = new CreateCommentCommandValidator();
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = GetValidCommentCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldReturnError_WhenNewCommentIsNull()
    {
        // Arrange
        var command = new CreateCommentCommand(null!, 1);
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("NewComment");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTextIsEmpty()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.Text = string.Empty;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("Text");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.Text)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTextIsNull()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.Text = null;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("Text");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.Text)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTextExceedsMaxLength()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.Text = new string('a', CreateCommentCommandValidator.MaxTextLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Text", CreateCommentCommandValidator.MaxTextLength);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.Text)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenTextIsAtMaxLength()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.Text = new string('a', CreateCommentCommandValidator.MaxTextLength);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.NewComment.Text);
    }

    [Fact]
    public void ShouldReturnError_WhenUserIdIsZero()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.UserId = 0;
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("UserId");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.UserId)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenUserIdIsNegative()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.UserId = -1;
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("UserId");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.UserId)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenStreetcodeIdIsZero()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.StreetcodeId = 0;
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("StreetcodeId");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.StreetcodeId)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenStreetcodeIdIsNegative()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.StreetcodeId = -5;
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("StreetcodeId");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.StreetcodeId)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenParentCommentIdIsNull()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.ParentCommentId = null;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.NewComment.ParentCommentId);
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenParentCommentIdIsValid()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.NewComment.ParentCommentId = 5;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.NewComment.ParentCommentId);
    }

    [Fact]
    public void ShouldReturnMultipleErrors_WhenMultipleFieldsAreInvalid()
    {
        // Arrange
        var command = new CreateCommentCommand(
            new CommentCreateDTO
            {
                Text = string.Empty,
                UserId = 0,
                StreetcodeId = -1
            }, 1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewComment.Text);
        result.ShouldHaveValidationErrorFor(c => c.NewComment.UserId);
        result.ShouldHaveValidationErrorFor(c => c.NewComment.StreetcodeId);
    }

    private static CreateCommentCommand GetValidCommentCommand()
    {
        return new CreateCommentCommand(
            new CommentCreateDTO
            {
                Text = "This is a valid comment text.",
                UserId = 1,
                StreetcodeId = 1,
                ParentCommentId = null
            }, 1);
    }
}
