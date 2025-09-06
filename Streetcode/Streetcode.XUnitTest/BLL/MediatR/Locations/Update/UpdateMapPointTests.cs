using System.Linq.Expressions;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Locations.Update;
using Streetcode.DAL.Entities.Analytics;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Locations.Update;

public class UpdateMapPointTests
{
    private readonly UpdateMapPointHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;

    public UpdateMapPointTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();

        _handler = new UpdateMapPointHandler(_repositoryWrapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_RecordExists_ShouldIncrementCount()
    {
        // Arrange
        var mapPoint = GetValidMapPoint();
        SetupRepositoryMocks(mapPoint, 1);
        var request = new UpdateMapPointCommand(mapPoint.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(6, mapPoint.Count);
        _repositoryWrapperMock.Verify(repo => repo.StatisticRecordRepository.Update(mapPoint), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RecordNotFound_ShouldReturnFail()
    {
        // Arrange
        var mapPoint = GetValidMapPoint();
        SetupRepositoryMocks(null);
        var request = new UpdateMapPointCommand(mapPoint.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("CannotFindRecordWithQrId", result.Errors[0].Message);
        _loggerMock.Verify(logger => logger.LogError(request, "CannotFindRecordWithQrId"), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveFails_ShouldReturnFail()
    {
        // Arrange
        var mapPoint = GetValidMapPoint();
        SetupRepositoryMocks(mapPoint, 0);
        var request = new UpdateMapPointCommand(mapPoint.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("CannotSaveTheData", result.Errors[0].Message);
        _loggerMock.Verify(logger => logger.LogError(request, "CannotSaveTheData"), Times.Once);
    }

    private StatisticRecord GetValidMapPoint()
    {
        return new StatisticRecord
        {
            Id = 1,
            Count = 5,
        };
    }

    private void SetupRepositoryMocks(StatisticRecord? mapPoint, int saveChanges = default)
    {
        _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null))
            .ReturnsAsync(mapPoint);

        if (mapPoint is not null)
        {
            _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.Update(It.IsAny<StatisticRecord>()));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(saveChanges);
        }
    }
}
