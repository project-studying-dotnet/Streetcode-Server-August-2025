using FluentResults;
using Streetcode.BLL.MediatR;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Other;

public class ResultFactoryTests
{
    [Fact]
    public void CreateFailure_WithGenericResult_ShouldReturnFailedGenericResult()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2" };

        // Act
        var result = ResultFactory.CreateFailure<Result<string>>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Message == "Error 1");
        Assert.Contains(result.Errors, e => e.Message == "Error 2");
    }

    [Fact]
    public void CreateFailure_WithNonGenericResult_ShouldReturnFailedResult()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2" };

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Message == "Error 1");
        Assert.Contains(result.Errors, e => e.Message == "Error 2");
    }

    [Fact]
    public void CreateFailure_WithEmptyMessages_ShouldReturnResultWithNoErrors()
    {
        // Arrange
        var messages = Array.Empty<string>();

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CreateFailure_WithNullMessages_ShouldReturnResultWithNoErrors()
    {
        // Arrange
        IEnumerable<string> messages = null;

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CreateFailure_WithNullAndEmptyMessages_ShouldFilterThemOut()
    {
        // Arrange
        var messages = new[] { "Valid Error", null, "", "  ", "Another Valid Error" };

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Message == "Valid Error");
        Assert.Contains(result.Errors, e => e.Message == "Another Valid Error");
    }

    [Fact]
    public void CreateFailure_WithDuplicateMessages_ShouldReturnDistinctErrors()
    {
        // Arrange
        var messages = new[] { "Duplicate Error", "Unique Error", "Duplicate Error", "Another Unique" };

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Message == "Duplicate Error");
        Assert.Contains(result.Errors, e => e.Message == "Unique Error");
        Assert.Contains(result.Errors, e => e.Message == "Another Unique");
    }

    [Fact]
    public void CreateFailure_WithDifferentGenericTypes_ShouldWork()
    {
        // Arrange
        var messages = new[] { "Error for int result" };

        // Act
        var intResult = ResultFactory.CreateFailure<Result<int>>(messages);
        var stringResult = ResultFactory.CreateFailure<Result<string>>(messages);
        var customResult = ResultFactory.CreateFailure<Result<CustomClass>>(messages);

        // Assert
        Assert.NotNull(intResult);
        Assert.True(intResult.IsFailed);
        Assert.Single(intResult.Errors);

        Assert.NotNull(stringResult);
        Assert.True(stringResult.IsFailed);
        Assert.Single(stringResult.Errors);

        Assert.NotNull(customResult);
        Assert.True(customResult.IsFailed);
        Assert.Single(customResult.Errors);
    }

    [Fact]
    public void CreateFailure_WithWhitespaceOnlyMessages_ShouldFilterThem()
    {
        // Arrange
        var messages = new[] { "   ", "\t", "\n", "Valid Error", "  \r\n  " };

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal("Valid Error", result.Errors.Single().Message);
    }

    [Fact]
    public void CreateFailure_WithSingleMessage_ShouldWork()
    {
        // Arrange
        var messages = new[] { "Single error message" };

        // Act
        var result = ResultFactory.CreateFailure<Result<bool>>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal("Single error message", result.Errors.Single().Message);
    }

    [Theory]
    [InlineData("Error 1")]
    [InlineData("Validation failed")]
    [InlineData("Custom error message")]
    public void CreateFailure_WithVariousErrorMessages_ShouldPreserveMessages(string errorMessage)
    {
        // Arrange
        var messages = new[] { errorMessage };

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMessage, result.Errors.Single().Message);
    }

    [Fact]
    public void CreateFailure_CaseSensitiveDistinct_ShouldTreatDifferentCasesAsDistinct()
    {
        // Arrange
        var messages = new[] { "Error", "error", "ERROR" };

        // Act
        var result = ResultFactory.CreateFailure<Result>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);

        // Should be distinct based on StringComparer.Ordinal
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void CreateFailure_WithComplexGenericType_ShouldWork()
    {
        // Arrange
        var messages = new[] { "Error with complex type" };

        // Act
        var result = ResultFactory.CreateFailure<Result<List<CustomClass>>>(messages);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal("Error with complex type", result.Errors.Single().Message);
    }
}

// Helper classes for testing
public class CustomClass
{
    public string Name { get; set; } = string.Empty;
}