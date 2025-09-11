using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Timeline;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class UpdateTimelineItemHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IHistoricalContextService> _historicalContextServiceMock;
        private readonly UpdateTimelineItemHandler _handler;

        public UpdateTimelineItemHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _historicalContextServiceMock = new Mock<IHistoricalContextService>();

            _handler = new UpdateTimelineItemHandler(
                _mapperMock.Object,
                _repositoryWrapperMock.Object,
                _loggerMock.Object,
                _historicalContextServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ReturnsSuccess()
        {
            // Arrange
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>
                {
                    new HistoricalContextRequestDto { Title = "Context A" },
                    new HistoricalContextRequestDto { Id = 11 }
                }
            });

            var timelineItem = new TimelineItemEntity
            {
                Id = 1,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>
                {
                    new HistoricalContextTimeline { TimelineId = 1, HistoricalContextId = 10 }
                }
            };

            var resultDto = new TimelineItemDTO
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextDTO>
                {
                    new HistoricalContextDTO { Id = 12, Title = "Context A" },
                    new HistoricalContextDTO { Id = 11, Title = "Context B" }
                }
            };

            SetupTimelineRepositoryGetFirstOrDefault(timelineItem);

            _mapperMock.Setup(m => m.Map(command.TimelineItem, timelineItem));

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupRemoveObsoleteLinks(Result.Ok());
            SetupBuildHistoricalContextLinksAsync(Result.Ok());

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.Update(timelineItem));
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<TimelineItemDTO>(timelineItem)).Returns(resultDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(result.Value, resultDto);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _repositoryWrapperMock.Verify(r => r.TimelineRepository.Update(timelineItem), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map(command.TimelineItem, timelineItem), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemDTO>(timelineItem), Times.Once);

            VerifyBuildHistoricalContextLinksAsync();
            VerifyCheckForDuplicateTitlesAsync();
            VerifyRemoveObsoleteLinks();

            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenTimelineItemNotFound_ReturnsFailAndLogsError()
        {
            // Arrange
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 99,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            });
            string errorMsg = Errors_Common.NotFoundById.FormatWith("timeline item", command.TimelineItem.Id);

            SetupTimelineRepositoryGetFirstOrDefault((TimelineItemEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenDuplicateContextTitle_ReturnsFailAndLogsError()
        {
            // Arrange
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>
                {
                    new HistoricalContextRequestDto { Id = null, Title = "Existing Context" }
                }
            });
            string errorMsg = Errors_Timeline.Context_TitleAlreadyExists.FormatWith("Existing Context");

            var timelineItem = new TimelineItemEntity { Id = 1 };

            SetupTimelineRepositoryGetFirstOrDefault(timelineItem);

            _mapperMock.Setup(m => m.Map(command.TimelineItem, timelineItem));

            SetupCheckForDuplicateTitlesAsync(Result.Fail(errorMsg));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _mapperMock.Verify(m => m.Map(command.TimelineItem, timelineItem), Times.Once);

            VerifyCheckForDuplicateTitlesAsync();

            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenRemoveObsoleteLinksFails_ReturnsFailAndLogsError()
        {
            // Arrange
            const string errorMsg = "Failed to remove obsolete links.";
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            });

            var timelineItem = new TimelineItemEntity { Id = 1 };

            SetupTimelineRepositoryGetFirstOrDefault(timelineItem);

            _mapperMock.Setup(m => m.Map(command.TimelineItem, timelineItem));

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupRemoveObsoleteLinks(Result.Fail(errorMsg));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _mapperMock.Verify(m => m.Map(command.TimelineItem, timelineItem), Times.Once);

            VerifyCheckForDuplicateTitlesAsync();
            VerifyRemoveObsoleteLinks();

            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenWhenBuildLinksFails_ReturnsFailAndLogsError()
        {
            // Arrange
            const string errorMsg = "Failed to build historical context links.";
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            });

            var timelineItem = new TimelineItemEntity { Id = 1 };

            SetupTimelineRepositoryGetFirstOrDefault(timelineItem);

            _mapperMock.Setup(m => m.Map(command.TimelineItem, timelineItem));

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupRemoveObsoleteLinks(Result.Ok());
            SetupBuildHistoricalContextLinksAsync(Result.Fail(errorMsg));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _mapperMock.Verify(m => m.Map(command.TimelineItem, timelineItem), Times.Once);

            VerifyBuildHistoricalContextLinksAsync();
            VerifyCheckForDuplicateTitlesAsync();
            VerifyRemoveObsoleteLinks();

            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ReturnsFailAndLogsError()
        {
            // Arrange
            string errorMsg = Errors_Common.FailedToUpdate.FormatWith("timeline item");
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            });
            var timelineItem = new TimelineItemEntity { Id = 1 };

            SetupTimelineRepositoryGetFirstOrDefault(timelineItem);

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupRemoveObsoleteLinks(Result.Ok());
            SetupBuildHistoricalContextLinksAsync(Result.Ok());

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.Update(timelineItem));
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _repositoryWrapperMock.Verify(r => r.TimelineRepository.Update(timelineItem), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map(command.TimelineItem, timelineItem), Times.Once);

            VerifyBuildHistoricalContextLinksAsync();
            VerifyCheckForDuplicateTitlesAsync();
            VerifyRemoveObsoleteLinks();
        }

        [Fact]
        public async Task Handle_WhenAnExceptionIsThrown_ReturnsFailAndLogsError()
        {
            // Arrange
            const string errorMsg = "Database connection lost";
            var command = new UpdateTimelineItemCommand(new TimelineItemUpdateDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            });

            var timelineItem = new TimelineItemEntity { Id = 1 };

            SetupTimelineRepositoryGetFirstOrDefault(timelineItem);

            _mapperMock.Setup(m => m.Map(command.TimelineItem, timelineItem)).Throws(new Exception(errorMsg));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);
            _mapperMock.Verify(m => m.Map(command.TimelineItem, timelineItem), Times.Once);

            VerifyTimelineItemGetFirstOrDefaultAsync();

            _mapperMock.VerifyNoOtherCalls();
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        private void SetupTimelineRepositoryGetFirstOrDefault(TimelineItemEntity timelineItem)
        {
            _repositoryWrapperMock.Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(timelineItem);
        }

        private void SetupCheckForDuplicateTitlesAsync(Result result)
        {
            _historicalContextServiceMock.Setup(s => s.CheckForDuplicateTitlesAsync(
                    It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(result);
        }

        private void SetupRemoveObsoleteLinks(Result result)
        {
            _historicalContextServiceMock.Setup(s => s.RemoveObsoleteLinks(
                    It.IsAny<TimelineItemEntity>(),
                    It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .Returns(result);
        }

        private void SetupBuildHistoricalContextLinksAsync(Result result)
        {
            _historicalContextServiceMock.Setup(s => s.BuildHistoricalContextLinksAsync(
                    It.IsAny<TimelineItemEntity>(),
                    It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(result);
        }

        private void VerifyTimelineItemGetFirstOrDefaultAsync()
        {
            _repositoryWrapperMock.Verify(
                r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);
        }

        private void VerifyCheckForDuplicateTitlesAsync()
        {
            _historicalContextServiceMock.Verify(
                s => s.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);
        }

        private void VerifyRemoveObsoleteLinks()
        {
            _historicalContextServiceMock.Verify(
                s => s.RemoveObsoleteLinks(
                It.IsAny<TimelineItemEntity>(),
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);
        }

        private void VerifyBuildHistoricalContextLinksAsync()
        {
            _historicalContextServiceMock.Verify(
                s => s.BuildHistoricalContextLinksAsync(
                It.IsAny<TimelineItemEntity>(),
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);
        }
    }
}
