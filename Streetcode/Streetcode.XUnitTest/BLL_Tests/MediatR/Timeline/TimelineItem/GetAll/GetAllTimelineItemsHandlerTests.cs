using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL_Tests.MediatR.Timeline.TimelineItem.GetAll;

public class GetAllTimelineItemsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllTimelineItemsHandler _handler;

    public GetAllTimelineItemsHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetAllTimelineItemsHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTimelineItemsExist_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new GetAllTimelineItemsQuery();
        var cancellationToken = CancellationToken.None;

        var (entities, mappedDtos) = CreateValidTimelineItemEntitiesAndDtos();

        SetupMocksForTimelineItems(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(mappedDtos.Count(), result.Value.Count());
        Assert.Equal(mappedDtos, result.Value);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_WhenTimelineItemsIsNull_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new GetAllTimelineItemsQuery();
        var cancellationToken = CancellationToken.None;
        const string errorMsg = "Cannot find any timelineItem";

        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetAllAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync((IEnumerable<TimelineItemEntity>?)null);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        _mockRepositoryWrapper.Verify(
            repo => repo.TimelineRepository.GetAllAsync(
            It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
            It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);

        _mockLogger.Verify(logger => logger.LogError(request, errorMsg), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTimelineItemsIsEmpty_ShouldReturnSuccessResultWithEmptyCollection()
    {
        // Arrange
        var request = new GetAllTimelineItemsQuery();
        var cancellationToken = CancellationToken.None;
        var (entities, mappedDtos) = CreateEmptyTimelineItemEntitiesAndDtos();

        SetupMocksForTimelineItems(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public void Constructor_WhenAllDependenciesProvided_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetAllTimelineItemsHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(handler);
    }

    private static (IEnumerable<TimelineItemEntity>, IEnumerable<TimelineItemDTO>) CreateValidTimelineItemEntitiesAndDtos()
    {
        var entities = new List<TimelineItemEntity>
        {
            new TimelineItemEntity
            {
                Id = 1,
                Date = new DateTime(2025, 1, 1),
                Title = "Test Timeline Item 1",
                Description = "Description 1"
            },
            new TimelineItemEntity
            {
                Id = 2,
                Date = new DateTime(2025, 2, 1),
                Title = "Test Timeline Item 2",
                Description = "Description 2"
            }
        };
        var mappedDtos = new List<TimelineItemDTO>
        {
            new TimelineItemDTO
            {
                Id = 1,
                Date = new DateTime(2025, 1, 1),
                Title = "Test Timeline Item 1",
                Description = "Description 1"
            },
            new TimelineItemDTO
            {
                Id = 2,
                Date = new DateTime(2025, 2, 1),
                Title = "Test Timeline Item 2",
                Description = "Description 2"
            }
        };

        return (entities, mappedDtos);
    }

    private static (IEnumerable<TimelineItemEntity>, IEnumerable<TimelineItemDTO>) CreateEmptyTimelineItemEntitiesAndDtos()
    {
        var entities = new List<TimelineItemEntity>();
        var mappedDtos = new List<TimelineItemDTO>();
        return (entities, mappedDtos);
    }

    private void SetupMocksForTimelineItems(
        IEnumerable<TimelineItemEntity> entities,
        IEnumerable<TimelineItemDTO> mappedDtos)
    {
        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetAllAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync(entities);
        _mockMapper
            .Setup(mapper => mapper.Map<IEnumerable<TimelineItemDTO>>(It.IsAny<IEnumerable<TimelineItemEntity>>()))
            .Returns(mappedDtos);
    }

    private void VerifyMocksCalledOnce()
    {
        _mockRepositoryWrapper.Verify(
            repo => repo.TimelineRepository.GetAllAsync(
            It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
            It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);

        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<TimelineItemDTO>>(It.IsAny<IEnumerable<TimelineItemEntity>>()), Times.Once);
    }
}
