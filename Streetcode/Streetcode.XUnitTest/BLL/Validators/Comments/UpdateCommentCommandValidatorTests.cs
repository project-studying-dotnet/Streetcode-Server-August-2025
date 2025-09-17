using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Comments;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Comments;

public class UpdateCommentCommandValidatorTests
{
    private readonly UpdateCommentCommandValidator _validator;

    public UpdateCommentCommandValidatorTests()
    {
        _validator = new UpdateCommentCommandValidator();
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
    public void ShouldReturnError_WhenCommentIsNull()
    {
        // Arrange
        var command = new UpdateCommentCommand(null!);
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("Comment");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldReturnError_WhenIdIsInvalid(int invalidId)
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.Comment.Id = invalidId;
        var expectedMessage = Errors_Validation.IsRequired.FormatWith("CommentId");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment.Id)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ShouldReturnError_WhenTextIsNullOrEmpty(string? invalidText)
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.Comment.Text = invalidText!;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("Text");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment.Text)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTextExceedsMaxLength()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.Comment.Text = new string('a', UpdateCommentCommandValidator.MaxTextLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Text", UpdateCommentCommandValidator.MaxTextLength);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment.Text)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenTextIsAtMaxLength()
    {
        // Arrange
        var command = GetValidCommentCommand();
        command.Comment.Text = new string('a', UpdateCommentCommandValidator.MaxTextLength);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Comment.Text);
    }

    [Fact]
    public void ShouldReturnMultipleErrors_WhenMultipleFieldsAreInvalid()
    {
        // Arrange
        var command = new UpdateCommentCommand(new CommentUpdateDTO
        {
            Id = 0,
            Text = string.Empty
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment.Id);
        result.ShouldHaveValidationErrorFor(c => c.Comment.Text);
    }

    private static UpdateCommentCommand GetValidCommentCommand()
    {
        return new UpdateCommentCommand(new CommentUpdateDTO
        {
            Id = 1,
            Text = "This is a valid updated comment text."
        });
    }
}