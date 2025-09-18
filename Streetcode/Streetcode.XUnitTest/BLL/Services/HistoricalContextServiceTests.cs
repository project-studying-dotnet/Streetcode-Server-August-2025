using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Services.Timeline;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

namespace Streetcode.XUnitTest.BLL.Services.Timeline
{
    public class HistoricalContextServiceTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly HistoricalContextService _service;

        public HistoricalContextServiceTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _service = new HistoricalContextService(_repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task CheckForDuplicateTitlesAsync_NoNewContexts_ReturnsSuccess()
        {
            // Arrange
            var contexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = 1 }
            };

            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.GetAllAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(new List<HistoricalContextEntity>());

            // Act
            var result = await _service.CheckForDuplicateTitlesAsync(contexts);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CheckForDuplicateTitlesAsync_NoDuplicatesFound_ReturnsSuccess()
        {
            // Arrange
            var newTitle = "New Context Title";
            var contexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = null, Title = newTitle }
            };

            var existingContexts = new List<HistoricalContextEntity>();

            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.GetAllAsync(
                 It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                 It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(existingContexts);

            // Act
            var result = await _service.CheckForDuplicateTitlesAsync(contexts);

            // Assert
            Assert.True(result.IsSuccess);

            _repositoryWrapperMock.Verify(
                r => r.HistoricalContextRepository.GetAllAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);
        }

        [Fact]
        public async Task CheckForDuplicateTitlesAsync_DuplicateFound_ReturnsFail()
        {
            // Arrange
            var newTitle = "Duplicate Title";
            string errorMessage = Errors_Timeline.Context_TitleAlreadyExists.FormatWith(newTitle);
            var contexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = null, Title = newTitle }
            };

            var existingContexts = new List<HistoricalContextEntity>
            {
                new HistoricalContextEntity { Id = 1, Title = newTitle }
            };

            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.GetAllAsync(
                 It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                 It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(existingContexts);

            // Act
            var result = await _service.CheckForDuplicateTitlesAsync(contexts);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMessage, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.HistoricalContextRepository.GetAllAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);
        }

        [Fact]
        public async Task CheckForDuplicateTitlesAsync_NullContexts_ReturnsFail()
        {
            // Arrange
            string errorMsg = Errors_Common.CannotBeNull.FormatWith("Input contexts");
            IEnumerable<HistoricalContextRequestDto> contexts = null!;

            // Act
            var result = await _service.CheckForDuplicateTitlesAsync(contexts);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BuildHistoricalContextLinksAsync_RemovesExistingLinks_WhenPresent()
        {
            // Arrange
            int timelineId = 1;
            int existingContextId = 10;
            var existingLink = new HistoricalContextTimeline
            {
                TimelineId = timelineId,
                HistoricalContextId = existingContextId
            };
            var timelineItem = new TimelineItem
            {
                Id = timelineId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline> { existingLink }
            };
            var contexts = new List<HistoricalContextRequestDto>();

            _repositoryWrapperMock.Setup(r => r.HistoricalContextTimelineRepository.DeleteRange(
                It.IsAny<IEnumerable<HistoricalContextTimeline>>()));

            // Act
            var result = await _service.BuildHistoricalContextLinksAsync(timelineItem, contexts);

            // Assert
            Assert.True(result.IsSuccess);

            _repositoryWrapperMock.Verify(
                r => r.HistoricalContextTimelineRepository.DeleteRange(
                It.IsAny<IEnumerable<HistoricalContextTimeline>>()), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BuildHistoricalContextLinksAsync_CreatesAndLinksNewContext_WhenDtoHasNoId()
        {
            // Arrange
            int timelineId = 1;
            int newContextId = 20;
            var timelineItem = new TimelineItem
            {
                Id = timelineId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>()
            };

            var newContextTitle = "New Historical Context";
            var contexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = null, Title = newContextTitle }
            };

            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.CreateAsync(It.IsAny<HistoricalContextEntity>()))
                .Returns(Task.FromResult(new HistoricalContextEntity { Id = newContextId }));

            // Act
            var result = await _service.BuildHistoricalContextLinksAsync(timelineItem, contexts);

            // Assert
            Assert.True(result.IsSuccess);

            _repositoryWrapperMock.Verify(r => r.HistoricalContextRepository.CreateAsync(It.IsAny<HistoricalContextEntity>()), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BuildHistoricalContextLinksAsync_LinksExistingContext_WhenDtoHasId()
        {
            // Arrange
            int timelineId = 1;
            int existingContextId = 10;
            var timelineItem = new TimelineItem
            {
                Id = timelineId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>()
            };
            var contexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = existingContextId }
            };
            var existingContext = new HistoricalContextEntity { Id = existingContextId };

            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync(existingContext);
            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.Attach(existingContext));

            // Act
            var result = await _service.BuildHistoricalContextLinksAsync(timelineItem, contexts);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(timelineItem.HistoricalContextTimelines);
            Assert.Equal(existingContextId, timelineItem.HistoricalContextTimelines[0].HistoricalContext!.Id);

            _repositoryWrapperMock.Verify(
                r => r.HistoricalContextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.HistoricalContextRepository.Attach(existingContext), Times.Once);

            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BuildHistoricalContextLinksAsync_ExistingContextNotFound_ReturnsFail()
        {
            // Arrange
            int timelineId = 1;
            int nonExistentId = -1;
            string errorMsg = Errors_Common.NotFoundById.FormatWith("Historical context", nonExistentId);
            var timelineItem = new TimelineItem
            {
                Id = timelineId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>()
            };
            var contexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = nonExistentId }
            };

            _repositoryWrapperMock.Setup(r => r.HistoricalContextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>, IIncludableQueryable<HistoricalContextEntity, object>>>()))
                .ReturnsAsync((HistoricalContextEntity)null!);

            // Act
            var result = await _service.BuildHistoricalContextLinksAsync(timelineItem, contexts);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            Assert.Empty(timelineItem.HistoricalContextTimelines);

            _repositoryWrapperMock.Verify(
                r => r.HistoricalContextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                IIncludableQueryable<HistoricalContextEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BuildHistoricalContextLinksAsync_NullContexts_ReturnsFail()
        {
            // Arrange
            int timelineId = 1;
            string errorMsg = Errors_Common.CannotBeNull.FormatWith("Input contexts");
            var timelineItem = new TimelineItem { Id = timelineId };

            // Act
            var result = await _service.BuildHistoricalContextLinksAsync(timelineItem, null!);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BuildHistoricalContextLinksAsync_NullTimelineItem_ReturnsFail()
        {
            // Arrange
            string errorMsg = Errors_Common.CannotBeNull.FormatWith("TimelineItem");
            IEnumerable<HistoricalContextRequestDto> contexts = new List<HistoricalContextRequestDto>();

            // Act
            var result = await _service.BuildHistoricalContextLinksAsync(null!, contexts);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void RemoveObsoleteLinks_WhenGivenValidInputs_RemovesOnlyObsoleteLinks()
        {
            // Arrange
            int timelineId = 1;
            int obsoleteContextId = 10;
            int newContextId = 20;

            var obsoleteLink = new HistoricalContextTimeline { TimelineId = timelineId, HistoricalContextId = obsoleteContextId };
            var existingLink = new HistoricalContextTimeline { TimelineId = timelineId, HistoricalContextId = newContextId };

            var timelineItem = new TimelineItem
            {
                Id = timelineId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline> { obsoleteLink, existingLink }
            };
            var newContexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = newContextId }
            };

            _repositoryWrapperMock.Setup(r => r.HistoricalContextTimelineRepository.DeleteRange(
                    It.IsAny<IEnumerable<HistoricalContextTimeline>>()));

            // Act
            var result = _service.RemoveObsoleteLinks(timelineItem, newContexts);

            // Assert
            Assert.True(result.IsSuccess);

            _repositoryWrapperMock.Verify(
                r => r.HistoricalContextTimelineRepository.DeleteRange(
                It.Is<IEnumerable<HistoricalContextTimeline>>(list => list.Contains(obsoleteLink))), Times.Once);
        }

        [Fact]
        public void RemoveObsoleteLinks_WhenNoObsoleteLinksExist_PerformsNoDeletion()
        {
            // Arrange
            int timelineId = 1;
            int existingContextId = 10;

            var timelineItem = new TimelineItem
            {
                Id = timelineId,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>
                {
                    new HistoricalContextTimeline { TimelineId = timelineId, HistoricalContextId = existingContextId }
                }
            };
            var newContexts = new List<HistoricalContextRequestDto>
            {
                new HistoricalContextRequestDto { Id = existingContextId }
            };

            // Act
            var result = _service.RemoveObsoleteLinks(timelineItem, newContexts);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void RemoveObsoleteLinks_WhenInputContextsAreNull_ReturnsFail()
        {
            // Arrange
            string errorMsg = Errors_Common.CannotBeNull.FormatWith("Input contexts");
            var timelineItem = new TimelineItem();

            // Act
            var result = _service.RemoveObsoleteLinks(timelineItem, null!);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void RemoveObsoleteLinks_WhenTimelineItemIsNull_ReturnsFail()
        {
            // Arrange
            string errorMsg = Errors_Common.CannotBeNull.FormatWith("TimelineItem");
            var newContexts = new List<HistoricalContextRequestDto>();

            // Act
            var result = _service.RemoveObsoleteLinks(null!, newContexts);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);
            _repositoryWrapperMock.VerifyNoOtherCalls();
        }
    }
}