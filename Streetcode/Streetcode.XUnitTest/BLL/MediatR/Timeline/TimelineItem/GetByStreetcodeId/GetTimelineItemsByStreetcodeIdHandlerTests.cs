using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;

public class GetTimelineItemsByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTimelineItemsByStreetcodeIdHandler _handler;

    public GetTimelineItemsByStreetcodeIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetTimelineItemsByStreetcodeIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTimelineItemsExistForStreetcodeId_ShouldReturnSuccessResult()
    {
        // Arrange
        int targetStreetcodeId = 1;
        var cancellationToken = CancellationToken.None;
        var request = new GetTimelineItemsByStreetcodeIdQuery(targetStreetcodeId);

        var (entities, mappedDtos) = CreateValidTimelineItemEntitiesAndDtos(targetStreetcodeId);

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
    public async Task Handle_WhenNoTimelineItemsFoundForStreetcodeId_ShouldReturnFailureResult()
    {
        // Arrange
        var streetcodeId = 999;
        var request = new GetTimelineItemsByStreetcodeIdQuery(streetcodeId);
        var cancellationToken = CancellationToken.None;
        var expectedErrorMessage = $"Cannot find any timeline item by the streetcode id: {streetcodeId}";

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
        Assert.Equal(expectedErrorMessage, result.Errors.First().Message);

        _mockLogger.Verify(
            logger => logger.LogError(request, expectedErrorMessage),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmptyCollectionReturned_ShouldReturnSuccessResultWithEmptyCollection()
    {
        // Arrange
        var streetcodeId = 1;
        var request = new GetTimelineItemsByStreetcodeIdQuery(streetcodeId);
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

        _mockLogger.Verify(
            logger => logger.LogError(It.IsAny<GetTimelineItemsByStreetcodeIdQuery>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Handle_WithDifferentStreetcodeIds_ShouldIncludeIdInErrorMessage(int streetcodeId)
    {
        var request = new GetTimelineItemsByStreetcodeIdQuery(streetcodeId);
        var cancellationToken = CancellationToken.None;
        var expectedErrorMessage = $"Cannot find any timeline item by the streetcode id: {streetcodeId}";

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
        Assert.Equal(expectedErrorMessage, result.Errors.First().Message);

        _mockLogger.Verify(
            logger => logger.LogError(request, expectedErrorMessage),
            Times.Once);
    }

    private static (IEnumerable<TimelineItemEntity>, IEnumerable<TimelineItemDTO>) CreateValidTimelineItemEntitiesAndDtos(int targetStreetcodeId)
    {
        var entities = new List<TimelineItemEntity>
        {
            new TimelineItemEntity
            {
                Id = 1,
                Date = new DateTime(2025, 1, 1),
                Title = "Test Timeline Item 1",
                Description = "Description 1",
                StreetcodeId = targetStreetcodeId
            },
            new TimelineItemEntity
            {
                Id = 2,
                Date = new DateTime(2025, 2, 1),
                Title = "Test Timeline Item 2",
                Description = "Description 2",
                StreetcodeId = targetStreetcodeId
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
