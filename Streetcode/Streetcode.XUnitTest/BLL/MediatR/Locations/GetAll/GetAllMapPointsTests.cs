using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Locations;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Locations.GetAll;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Locations.GetAll;

public class GetAllMapPointsTests
{
    private readonly GetAllMapPointsHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;

    public GetAllMapPointsTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetAllMapPointsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_RecordsExist_ShouldReturnOrderedRecords()
    {
        // Arrange
        var mapPoints = GetMapPoints();
        var mapPointsDTOs = GetMapPointsDTOs();

        SetupRepositoryMocks(mapPoints);
        SetupLoggerMocks(mapPointsDTOs);

        var request = new GetAllMapPointsQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryWrapperMock.Verify(
            repo => repo.StatisticRecordRepository.GetAllAsync(
                It.IsAny<Expression<Func<StatisticRecord, bool>>>(),
                It.IsAny<Func<IQueryable<StatisticRecord>, IIncludableQueryable<StatisticRecord, object>>>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<MapPointDTO>>(It.IsAny<IEnumerable<StatisticRecord>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RecordsAreNull_ShouldReturnFail()
    {
        // Arrange
        SetupRepositoryMocks(null);

        var request = new GetAllMapPointsQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("CannotGetPoints", result.Errors[0].Message);
        _loggerMock.Verify(logger => logger.LogError(request, "CannotGetPoints"), Times.Once);
    }

    [Fact]
    public async Task Handle_MappingFails_ShouldReturnFail()
    {
        // Arrange
        var mapPoints = GetMapPoints();

        SetupRepositoryMocks(mapPoints);
        SetupLoggerMocks(null);

        var request = new GetAllMapPointsQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("CannotMapPoints", result.Errors[0].Message);
        _loggerMock.Verify(logger => logger.LogError(request, "CannotMapPoints"), Times.Once);
    }

    private List<StatisticRecord> GetMapPoints()
    {
        return new List<StatisticRecord>
        {
            new () { Id = 1, QrId = 1, Count = 10 },
            new () { Id = 2, QrId = 2, Count = 20 },
            new () { Id = 3, QrId = 3, Count = 15 },
        };
    }

    private List<MapPointDTO> GetMapPointsDTOs()
    {
        return new List<MapPointDTO>
        {
            new () { Id = 1, PlateNumber = 10 },
            new () { Id = 2, PlateNumber = 20 },
            new () { Id = 3, PlateNumber = 15 },
        };
    }

    private void SetupRepositoryMocks(List<StatisticRecord>? mapPoints)
    {
        _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.GetAllAsync(
            It.IsAny<Expression<Func<StatisticRecord, bool>>>(),
            It.IsAny<Func<IQueryable<StatisticRecord>, IIncludableQueryable<StatisticRecord, object>>>()))
            .ReturnsAsync(mapPoints!);
    }

    private void SetupLoggerMocks(List<MapPointDTO>? mapPoints)
    {
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MapPointDTO>>(It.IsAny<IEnumerable<StatisticRecord>>()))
            .Returns(mapPoints!);
    }
}
