using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Timeline;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Streetcode;
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

            int streetcodeId = 1;
            var command = new CreateTimelineItemCommand(streetcodeId, createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            var timelineItemDto = new TimelineItemDTO { Title = createTimelineItem.Title, Description = createTimelineItem.Description };

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupBuildHistoricalContextLinksAsync(Result.Ok());
            SetupStreetcodeRepositoryGetFirstOrDefault(new StreetcodeContent { Id = streetcodeId });

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

            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemDTO>(timelineItemEntity), Times.Once);

            VerifyCheckForDuplicateTitlesAsync();
            VerifyBuildHistoricalContextLinksAsync();
            VerifyStreetcodeRepositoryGetFirstOrDefaultAsync();
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenMappingReturnsNull_ReturnsFailAndLogsError()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            int streetcodeId = 1;
            var command = new CreateTimelineItemCommand(streetcodeId, createTimelineItem);
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
        public async Task Handle_WhenStreetcodeIsNotFound_ReturnsFailAndLogsError()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            int nonExistentStreetcodeId = -1;
            var command = new CreateTimelineItemCommand(nonExistentStreetcodeId, createTimelineItem);
            string errorMsg = Errors_Common.NotFoundById.FormatWith("Streetcode", nonExistentStreetcodeId);

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(new TimelineItemEntity());
            SetupStreetcodeRepositoryGetFirstOrDefault(null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            VerifyStreetcodeRepositoryGetFirstOrDefaultAsync();
            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenHistoricalContextValidationFails_ReturnsFailAndLogsError()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            int streetcodeId = 1;
            var command = new CreateTimelineItemCommand(streetcodeId, createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            const string expectedError = "Duplicate historical context found.";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            SetupCheckForDuplicateTitlesAsync(Result.Fail(expectedError));
            SetupStreetcodeRepositoryGetFirstOrDefault(new StreetcodeContent { Id = streetcodeId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);

            VerifyCheckForDuplicateTitlesAsync();
            VerifyStreetcodeRepositoryGetFirstOrDefaultAsync();
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenBuildLinksFails_ReturnsFailAndLogsError()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            int streetcodeId = 1;
            var command = new CreateTimelineItemCommand(streetcodeId, createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            const string expectedError = "Failed to build historical context links.";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupBuildHistoricalContextLinksAsync(Result.Fail(expectedError));
            SetupStreetcodeRepositoryGetFirstOrDefault(new StreetcodeContent { Id = streetcodeId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);
            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);

            VerifyCheckForDuplicateTitlesAsync();
            VerifyBuildHistoricalContextLinksAsync();
            VerifyStreetcodeRepositoryGetFirstOrDefaultAsync();
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ReturnsFailAndLogsError()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            int streetcodeId = 1;
            var command = new CreateTimelineItemCommand(streetcodeId, createTimelineItem);
            var timelineItemEntity = new TimelineItemEntity { Title = createTimelineItem.Title, Description = createTimelineItem.Description };
            const string expectedError = "Failed to create a timeline item";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Returns(timelineItemEntity);

            _repositoryWrapperMock.Setup(r => r.TimelineRepository.CreateAsync(timelineItemEntity)).ReturnsAsync(timelineItemEntity);

            SetupCheckForDuplicateTitlesAsync(Result.Ok());
            SetupBuildHistoricalContextLinksAsync(Result.Ok());

            SetupStreetcodeRepositoryGetFirstOrDefault(new StreetcodeContent { Id = streetcodeId });

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(expectedError, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, expectedError), Times.Once);

            _repositoryWrapperMock.Verify(r => r.TimelineRepository.CreateAsync(timelineItemEntity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            VerifyCheckForDuplicateTitlesAsync();
            VerifyBuildHistoricalContextLinksAsync();

            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            VerifyStreetcodeRepositoryGetFirstOrDefaultAsync();
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenAnExceptionIsThrown_ReturnsFailAndLogsError()
        {
            // Arrange
            var createTimelineItem = new TimelineItemBaseDto
            {
                Title = "Title",
                Description = "Description",
                HistoricalContexts = new List<HistoricalContextRequestDto>()
            };
            int streetcodeId = 1;
            var command = new CreateTimelineItemCommand(streetcodeId, createTimelineItem);
            var exceptionMessage = "Database connection lost.";

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(command.TimelineItem)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(exceptionMessage, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(command, exceptionMessage), Times.Once);

            _mapperMock.Verify(m => m.Map<TimelineItemEntity>(command.TimelineItem), Times.Once);
            _loggerMock.Verify(l => l.LogError(command, exceptionMessage), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _historicalContextServiceMock.VerifyNoOtherCalls();
        }

        private void SetupStreetcodeRepositoryGetFirstOrDefault(StreetcodeContent streetcode)
        {
            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(streetcode);
        }

        private void SetupCheckForDuplicateTitlesAsync(Result result)
        {
            _historicalContextServiceMock.Setup(s => s.CheckForDuplicateTitlesAsync(
                    It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(result);
        }

        private void SetupBuildHistoricalContextLinksAsync(Result result)
        {
            _historicalContextServiceMock.Setup(s => s.BuildHistoricalContextLinksAsync(
                    It.IsAny<TimelineItemEntity>(),
                    It.IsAny<IEnumerable<HistoricalContextRequestDto>>()))
                .ReturnsAsync(result);
        }

        private void VerifyCheckForDuplicateTitlesAsync()
        {
            _historicalContextServiceMock.Verify(
                s => s.CheckForDuplicateTitlesAsync(
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);
        }

        private void VerifyBuildHistoricalContextLinksAsync()
        {
            _historicalContextServiceMock.Verify(
                s => s.BuildHistoricalContextLinksAsync(
                It.IsAny<TimelineItemEntity>(),
                It.IsAny<IEnumerable<HistoricalContextRequestDto>>()), Times.Once);
        }

        private void VerifyStreetcodeRepositoryGetFirstOrDefaultAsync()
        {
            _repositoryWrapperMock.Verify(
                r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()), Times.Once);
        }
    }
}
