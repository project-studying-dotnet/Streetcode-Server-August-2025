using System.Linq.Expressions;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Delete;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Streetcode.Delete;

public class DeleteStreetcodeHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly DeleteStreetcodeHandler _handler;

    public DeleteStreetcodeHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new DeleteStreetcodeHandler(
            _repositoryWrapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_DeletesStreetcodeSuccessfully()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcodeContent = new StreetcodeContent { Id = streetcodeId };
        SetupRepositoryMocks(streetcodeContent, saveChanges: 1);
        var command = new DeleteStreetcodeCommand(streetcodeId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _repositoryWrapperMock.Verify(r => r.StreetcodeRepository.Delete(streetcodeContent), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_InvalidId_ReturnsFailureResult()
    {
        // Arrange
        int streetcodeId = 999;
        SetupRepositoryMocks(null, saveChanges: 0);
        var command = new DeleteStreetcodeCommand(streetcodeId);
        string expectedErrorMessage = $"Cannot find any Streetcode with corresponding Id: {streetcodeId}";

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        _repositoryWrapperMock.Verify(r => r.StreetcodeRepository.Delete(It.IsAny<StreetcodeContent>()), Times.Never);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        _loggerMock.Verify(l => l.LogError(command, expectedErrorMessage), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncFails_ReturnsFailureResult()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcodeContent = new StreetcodeContent { Id = streetcodeId };
        SetupRepositoryMocks(streetcodeContent, saveChanges: -1);
        var command = new DeleteStreetcodeCommand(streetcodeId);
        string expectedErrorMessage = "Failed to delete the Streetcode";

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        _repositoryWrapperMock.Verify(r => r.StreetcodeRepository.Delete(streetcodeContent), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _loggerMock.Verify(l => l.LogError(command, expectedErrorMessage), Times.Once);
    }

    private void SetupRepositoryMocks(StreetcodeContent? streetcodeContent, int saveChanges)
    {
        _repositoryWrapperMock.Setup(
            r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(streetcodeContent);

        if (streetcodeContent != null)
        {
            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.Delete(streetcodeContent));
        }

        _repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(saveChanges);
    }
}
