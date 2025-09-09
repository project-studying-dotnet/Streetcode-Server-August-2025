using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

namespace Streetcode.XUnitTest.BLL.MediatR.Timeline.HistoricalContext
{
    public class DeleteHistoricalContextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DeleteHistoricalContextHandler _handler;

        public DeleteHistoricalContextHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new DeleteHistoricalContextHandler(
                _repositoryWrapperMock.Object,
                _loggerMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            int validId = 1;

            var command = new DeleteHistoricalContextCommand(validId);

            var historicalContext = new HistoricalContextEntity
            {
                Id = validId,
                Title = "Valid Context",
                HistoricalContextTimelines = new List<HistoricalContextTimeline>()
            };
            var expectedDto = new HistoricalContextDTO { Id = validId, Title = "Valid Context" };

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(historicalContext);

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.Delete(historicalContext));
            _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(x => x.Map<HistoricalContextDTO>(historicalContext)).Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDto, result.Value);

            _repositoryWrapperMock.Verify(
                x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.Verify(x => x.HistoricalContextRepository.Delete(historicalContext), Times.Once);
            _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(x => x.Map<HistoricalContextDTO>(historicalContext), Times.Once);

            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ContextInUse_ReturnsFailAndLogsError()
        {
            // Arrange
            int inUseId = 2;
            var errorMsg = Errors_Timeline.CannotDeleteHistoricalContextInUse;
            var command = new DeleteHistoricalContextCommand(inUseId);
            var historicalContext = new HistoricalContextEntity
            {
                Id = inUseId,
                Title = "In Use Context",
                HistoricalContextTimelines = new List<HistoricalContextTimeline>
                {
                    new HistoricalContextTimeline { TimelineId = 1, HistoricalContextId = inUseId }
                }
            };

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(historicalContext);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);

            _loggerMock.Verify(x => x.LogError(command, errorMsg), Times.Once);
            _repositoryWrapperMock.Verify(x => x.HistoricalContextRepository.Delete(It.IsAny<HistoricalContextEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ContextNotFound_ReturnsFailAndLogsError()
        {
            // Arrange
            int nonExistentId = -1;
            var errorMsg = Errors_Common.NotFoundById.FormatWith("historical context", nonExistentId);
            var command = new DeleteHistoricalContextCommand(nonExistentId);

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync((HistoricalContextEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);

            _loggerMock.Verify(x => x.LogError(command, errorMsg), Times.Once);
            _repositoryWrapperMock.Verify(x => x.HistoricalContextRepository.Delete(It.IsAny<HistoricalContextEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ReturnsFailAndLogsError()
        {
            // Arrange
            int validId = 3;
            var errorMsg = Errors_Common.FailedToDelete.FormatWith("historical context");

            var command = new DeleteHistoricalContextCommand(validId);

            var historicalContext = new HistoricalContextEntity
            {
                Id = validId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>()
            };

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(historicalContext);

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.Delete(historicalContext));
            _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);

            _loggerMock.Verify(x => x.LogError(command, errorMsg), Times.Once);
            _repositoryWrapperMock.Verify(x => x.HistoricalContextRepository.Delete(historicalContext), Times.Once);
            _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_UnexpectedException_ReturnsFailAndLogsError()
        {
            // Arrange
            int validId = 4;
            const string errorMsg = "Simulated database error.";
            var command = new DeleteHistoricalContextCommand(validId);

            _repositoryWrapperMock.Setup(x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ThrowsAsync(new InvalidOperationException(errorMsg));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                x => x.HistoricalContextRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.VerifyNoOtherCalls();
            _loggerMock.Verify(x => x.LogError(command, errorMsg), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
