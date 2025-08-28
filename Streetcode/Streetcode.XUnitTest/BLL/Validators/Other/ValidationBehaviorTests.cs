using FluentAssertions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Streetcode.BLL.MediatR;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Other;

public class ValidationBehaviorTests
{
    public class TestRequest : IRequest<Result<string>>
    {
        public string Name { get; set; } = string.Empty;
    }

    // Helper: create delegate that just returns success
    private static RequestHandlerDelegate<Result<string>> SuccessDelegate(string value = "OK")
        => () => Task.FromResult(Result.Ok(value));

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result<string>>(Enumerable.Empty<IValidator<TestRequest>>());

        var request = new TestRequest { Name = "valid" };

        // Act
        var result = await behavior.Handle(request, SuccessDelegate("NextCalled"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("NextCalled");
    }

    [Fact]
    public async Task Handle_ValidatorPasses_CallsNext()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, Result<string>>(new[] { validator.Object });

        var request = new TestRequest { Name = "valid" };

        // Act
        var result = await behavior.Handle(request, SuccessDelegate("NextCalled"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("NextCalled");
    }

    [Fact]
    public async Task Handle_ValidatorFails_ReturnsFailure()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

        var validator = new Mock<IValidator<TestRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, Result<string>>(new[] { validator.Object });

        var request = new TestRequest();

        // Act
        var result = await behavior.Handle(request, SuccessDelegate("ShouldNotBeCalled"), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Name is required"));
    }

    [Fact]
    public async Task Handle_MultipleValidatorsWithSameError_Deduplicates()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

        var validator1 = new Mock<IValidator<TestRequest>>();
        validator1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var validator2 = new Mock<IValidator<TestRequest>>();
        validator2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, Result<string>>(new[] { validator1.Object, validator2.Object });

        var request = new TestRequest();

        // Act
        var result = await behavior.Handle(request, SuccessDelegate("ShouldNotBeCalled"), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Name is required"));
    }
}