using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public class DeleteTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteTimelineItemHandler _handler;

        public DeleteTimelineItemHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteTimelineItemHandler(
                _repositoryWrapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenItemIsSuccessfullyDeleted_ReturnsSuccess()
        {
            // Arrange
            var command = new DeleteTimelineItemCommand(1);
            var timelineItemEntity = new TimelineItemEntity
            {
                Id = 1,
                Title = "Title",
                HistoricalContextTimelines = new List<HistoricalContextTimeline>()
            };
            var timelineItemDto = new TimelineItemDTO
            {
                Id = timelineItemEntity.Id,
                Title = timelineItemEntity.Title,
            };

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(timelineItemEntity);

            _repositoryWrapperMock.Setup(r => r.HistoricalContextTimelineRepository.DeleteRange(It.IsAny<IEnumerable<HistoricalContextTimeline>>()));
            _repositoryWrapperMock.Setup(r => r.TimelineRepository.Delete(It.IsAny<TimelineItemEntity>()));
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _repositoryWrapperMock.Verify(
                r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.Verify(r => r.HistoricalContextTimelineRepository.DeleteRange(timelineItemEntity.HistoricalContextTimelines), Times.Once);
            _repositoryWrapperMock.Verify(r => r.TimelineRepository.Delete(timelineItemEntity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenItemIsNotFound_ReturnsFailAndLogsError()
        {
            // Arrange
            var command = new DeleteTimelineItemCommand(-1);
            var expectedError = Errors_Common.NotFoundById.FormatWith("timeline item", command.id);

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync((TimelineItemEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ReturnsFailAndLogsError()
        {
            // Arrange
            var request = new DeleteTimelineItemCommand(1);
            var timelineItemEntity = new TimelineItemEntity { HistoricalContextTimelines = new List<HistoricalContextTimeline>() };
            var expectedError = Errors_Common.FailedToDelete.FormatWith("timeline item");

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(timelineItemEntity);

            _repositoryWrapperMock.Setup(r => r.HistoricalContextTimelineRepository.DeleteRange(It.IsAny<IEnumerable<HistoricalContextTimeline>>()));
            _repositoryWrapperMock.Setup(r => r.TimelineRepository.Delete(It.IsAny<TimelineItemEntity>()));
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.Verify(r => r.HistoricalContextTimelineRepository.DeleteRange(timelineItemEntity.HistoricalContextTimelines), Times.Once);
            _repositoryWrapperMock.Verify(r => r.TimelineRepository.Delete(It.IsAny<TimelineItemEntity>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(l => l.LogError(request, expectedError), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenAnExceptionIsThrown_ReturnsFailResult()
        {
            // Arrange
            var request = new DeleteTimelineItemCommand(1);
            var exceptionMessage = "Database connection lost.";

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(exceptionMessage, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.VerifyNoOtherCalls();
            _loggerMock.Verify(l => l.LogError(request, exceptionMessage), Times.Once);
        }
    }
}