using AutoMapper;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Timeline;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateTimelineItemHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IHistoricalContextService> _historicalContextServiceMock;
        private readonly CreateTimelineItemHandler _handler;

        public CreateTimelineItemHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _historicalContextServiceMock = new Mock<IHistoricalContextService>();
            _handler = new CreateTimelineItemHandler(
                _mapperMock.Object,
                _repositoryWrapperMock.Object,
                _loggerMock.Object,
                _historicalContextServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenCreationIsSuccessful_ReturnsOkResultWithMappedDto()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            var timelineItemDto = new TimelineItemDTO { Title = createTimelineItem.Title, Description = createTimelineItem.Description };

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);
            _historicalContextServiceMock.Setup(x => x.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(Result.Ok());

            _historicalContextServiceMock.Setup(x => x.BuildHistoricalContextLinksAsync(
                timelineItemEntity, It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(Result.Ok());

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.CreateAsync(timelineItemEntity)).ReturnsAsync(timelineItemEntity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<TimelineItemDTO>(timelineItemEntity)).Returns(timelineItemDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(timelineItemDto, result.Value);

            _repositoryWrapperMock.Verify(r => r.TimelineRepository.CreateAsync(timelineItemEntity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            _historicalContextServiceMock.Verify(
                x => x.CheckForDuplicateTitlesAsync(
                command.TimelineItem.HistoricalContexts!), Times.Once);

            _historicalContextServiceMock.Verify(
                x => x.BuildHistoricalContextLinksAsync(
                timelineItemEntity, command.TimelineItem.HistoricalContexts!), Times.Once);

            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemDTO>(timelineItemEntity), Times.Once);

            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenMappingReturnsNull_ReturnsFailResult()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
            };
            var command = new CreateTimelineItemCommand(createTimelineItem);
            const string expectedError = "Cannot convert null to timeline item";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns((TimelineItemEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenHistoricalContextValidationFails_ReturnsFailResult()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            const string expectedError = "Duplicate historical context found.";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            _historicalContextServiceMock.Setup(x => x.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>())).ReturnsAsync(Result.Fail(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);

            _historicalContextServiceMock.Verify(
                x => x.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);

            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenBuildLinksFails_ReturnsFailResult()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            const string expectedError = "Failed to build historical context links.";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            _historicalContextServiceMock.Setup(x => x.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>())).ReturnsAsync(Result.Ok());

            _historicalContextServiceMock.Setup(x => x.BuildHistoricalContextLinksAsync(
                timelineItemEntity, It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(Result.Fail(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);

            _historicalContextServiceMock.Verify(
                x => x.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);

            _historicalContextServiceMock.Verify(
               x => x.BuildHistoricalContextLinksAsync(
               timelineItemEntity, It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);

            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ReturnsFailResult()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            const string expectedError = "Failed to create a timeline item";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.CreateAsync(timelineItemEntity)).ReturnsAsync(timelineItemEntity);

            _historicalContextServiceMock.Setup(x => x.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(Result.Ok());

            _historicalContextServiceMock.Setup(x => x.BuildHistoricalContextLinksAsync(
                timelineItemEntity, It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(Result.Ok());

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);

            _repositoryWrapperMock.Verify(r => r.TimelineRepository.CreateAsync(timelineItemEntity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            _historicalContextServiceMock.Verify(
                x => x.CheckForDuplicateTitlesAsync(
                command.TimelineItem.HistoricalContexts!), Times.Once);

            _historicalContextServiceMock.Verify(
                x => x.BuildHistoricalContextLinksAsync(
                timelineItemEntity, command.TimelineItem.HistoricalContexts!), Times.Once);

            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenAnExceptionIsThrown_ReturnsFailResult()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };

            var command = new CreateTimelineItemCommand(createTimelineItem);
            var exceptionMessage = "Database connection lost.";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(exceptionMessage, result.Errors[0].Message);

            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _loggerMock.Verify(l => l.LogError(command, exceptionMessage), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }
    }
}
