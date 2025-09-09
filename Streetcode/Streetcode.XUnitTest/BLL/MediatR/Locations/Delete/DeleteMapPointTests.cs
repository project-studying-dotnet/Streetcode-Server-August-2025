using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Locations.Delete;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Locations.Delete;

public class DeleteMapPointTests
{
    private readonly DeleteMapPointHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;

    public DeleteMapPointTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();

        _handler = new DeleteMapPointHandler(_repositoryWrapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_RecordIsDeletedSuccessfully_ShouldReturnSuccess()
    {
        // Arrange
        var mapPoint = GetValidMapPoint();
        SetupRepositoryMocks(mapPoint, 1);
        var request = new DeleteMapPointCommand(mapPoint.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Unit.Value, result.Value);
        _repositoryWrapperMock.Verify(repo => repo.StatisticRecordRepository.Delete(It.IsAny<StatisticRecord>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RecordIsNotFound_ShouldReturnError()
    {
        // Arrange
        var mapPoint = GetValidMapPoint(-1);
        SetupRepositoryMocks(null);
        var request = new DeleteMapPointCommand(mapPoint.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("CannotFindPointWithId", result.Errors[0].Message);
        _repositoryWrapperMock.Verify(repo => repo.StatisticRecordRepository.Delete(It.IsAny<StatisticRecord>()), Times.Never);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_DeleteFails_ShouldReturnError()
    {
        // Arrange
        var mapPoint = GetValidMapPoint();
        SetupRepositoryMocks(mapPoint, 0);
        var request = new DeleteMapPointCommand(mapPoint.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("FailedToDeleteThePoint", result.Errors[0].Message);
        _repositoryWrapperMock.Verify(repo => repo.StatisticRecordRepository.Delete(It.IsAny<StatisticRecord>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    private static StatisticRecord GetValidMapPoint(int id = 1)
    {
        return new StatisticRecord
        {
            Id = id
        };
    }

    private void SetupRepositoryMocks(StatisticRecord? mapPoint, int saveChanges = default)
    {
        _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StatisticRecord, bool>>>(),
            It.IsAny<Func<IQueryable<StatisticRecord>, IIncludableQueryable<StatisticRecord, object>>>()))
            .ReturnsAsync(mapPoint);

        if (mapPoint is not null)
        {
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(saveChanges);
        }
    }
}
