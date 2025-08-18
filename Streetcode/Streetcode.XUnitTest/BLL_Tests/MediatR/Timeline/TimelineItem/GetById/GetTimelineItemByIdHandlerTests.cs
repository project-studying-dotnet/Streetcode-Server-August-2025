using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL_Tests.MediatR.Timeline.TimelineItem.GetById;

public class GetTimelineItemByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTimelineItemByIdHandler _handler;

    public GetTimelineItemByIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetTimelineItemByIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTimelineItemExists_ShouldReturnSuccessResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var (entity, mappedDto, requestId) = CreateValidTimelineItemEntityAndDto();
        var request = new GetTimelineItemByIdQuery(requestId);

        SetupMocksForTimelineItems(entity, mappedDto);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(requestId, result.Value.Id);
        Assert.Equal(entity.Title, result.Value.Title);
        Assert.Equal(mappedDto, result.Value);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_WhenTimelineItemDoesNotExist_ShouldReturnFailureResult()
    {
        // Arrange
        var requestId = 999;
        var request = new GetTimelineItemByIdQuery(requestId);
        var cancellationToken = CancellationToken.None;
        var expectedErrorMessage = $"Cannot find a timeline item with corresponding id: {requestId}";

        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync((TimelineItemEntity?)null);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(expectedErrorMessage, result.Errors.First().Message);

        _mockLogger.Verify(
            logger => logger.LogError(request, expectedErrorMessage),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(-1)]
    public async Task Handle_WithDifferentIds_ShouldIncludeIdInErrorMessage(int requestId)
    {
        // Arrange
        var request = new GetTimelineItemByIdQuery(requestId);
        var cancellationToken = CancellationToken.None;
        var expectedErrorMessage = $"Cannot find a timeline item with corresponding id: {requestId}";

        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync((TimelineItemEntity?)null);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(expectedErrorMessage, result.Errors.First().Message);

        _mockLogger.Verify(
            logger => logger.LogError(request, expectedErrorMessage),
            Times.Once);
    }

    private static (TimelineItemEntity, TimelineItemDTO, int) CreateValidTimelineItemEntityAndDto()
    {
        const int targetTimelineItemId = 1;

        var entity = new TimelineItemEntity
        {
            Id = targetTimelineItemId,
            Date = new DateTime(2025, 1, 1),
            Title = "Test Timeline Item 1",
            Description = "Description 1",
            HistoricalContextTimelines = new List<HistoricalContextTimeline>
            {
                new HistoricalContextTimeline
                {
                    HistoricalContext = new HistoricalContext
                    {
                        Id = 1,
                        Title = "Historical Context 1"
                    }
                }
            }
        };
        var mappedDto = new TimelineItemDTO
        {
            Id = targetTimelineItemId,
            Date = new DateTime(2025, 1, 1),
            Title = "Test Timeline Item 1",
            Description = "Description 1",
            HistoricalContexts = new List<HistoricalContextDTO>
            {
                new HistoricalContextDTO
                {
                    Id = 1,
                    Title = "Historical Context 1"
                }
            }
        };

        return (entity, mappedDto, targetTimelineItemId);
    }

    private void SetupMocksForTimelineItems(
        TimelineItemEntity entity,
        TimelineItemDTO mappedDto)
    {
        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync(entity);
        _mockMapper
            .Setup(mapper => mapper.Map<TimelineItemDTO>(It.IsAny<TimelineItemEntity>()))
            .Returns(mappedDto);
    }

    private void VerifyMocksCalledOnce()
    {
        _mockRepositoryWrapper.Verify(
            repo => repo.TimelineRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
            It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);

        _mockMapper.Verify(mapper => mapper.Map<TimelineItemDTO>(It.IsAny<TimelineItemEntity>()), Times.Once);
    }
}
