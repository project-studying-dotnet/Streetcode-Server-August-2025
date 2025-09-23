using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.GetById;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments.GetById
{
    public class GetCommentByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetCommentByIdHandler _handler;

        public GetCommentByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetCommentByIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingComment_ReturnsSuccessAndMappedDto()
        {
            // Arrange
            var testCommentId = 1;
            var testComment = new CommentContent { Id = testCommentId };
            var expectedDto = new CommentDTO { Id = testCommentId };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(testCommentId))
                .ReturnsAsync(testComment);
            _mapperMock.Setup(m => m.Map<CommentDTO>(testComment))
                .Returns(expectedDto);

            var query = new GetCommentByIdQuery(testCommentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDto, result.Value);
            _repositoryWrapperMock.Verify(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(testCommentId), Times.Once);
            _mapperMock.Verify(m => m.Map<CommentDTO>(testComment), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_NonExistingComment_ReturnsFailAndLogsError()
        {
            // Arrange
            var nonExistingCommentId = 999;
            string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", nonExistingCommentId);

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(nonExistingCommentId))
                .ReturnsAsync((CommentContent)null!);

            var query = new GetCommentByIdQuery(nonExistingCommentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message == errorMsg);

            _loggerMock.Verify(l => l.LogError(query, errorMsg), Times.Once);
            _repositoryWrapperMock.Verify(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(nonExistingCommentId), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithIsReviewedFilterTrue_ReturnsFilteredReplies()
        {
            // Arrange
            var commentId = 1;
            var rootComment = new CommentContent
            {
                Id = commentId,
                Replies = new List<CommentContent>
                {
                    new CommentContent { Id = 2, IsReviewed = true },
                    new CommentContent { Id = 3, IsReviewed = false },
                    new CommentContent { Id = 4, IsReviewed = true }
                }
            };
            var mappedDto = new CommentDTO
            {
                Id = commentId,
                Replies = new List<CommentDTO>
                {
                    new CommentDTO { Id = 2, IsReviewed = true },
                    new CommentDTO { Id = 4, IsReviewed = true }
                }
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(commentId))
                .ReturnsAsync(rootComment);
            _mapperMock.Setup(m => m.Map<CommentDTO>(rootComment))
                .Returns(mappedDto);

            var query = new GetCommentByIdQuery(commentId, true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(mappedDto, result.Value);

            _repositoryWrapperMock.Verify(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(commentId), Times.Once);
            _mapperMock.Verify(m => m.Map<CommentDTO>(rootComment), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithIsReviewedFilterFalse_ReturnsFilteredReplies()
        {
            // Arrange
            var commentId = 1;
            var rootComment = new CommentContent
            {
                Id = commentId,
                Replies = new List<CommentContent>
                {
                    new CommentContent { Id = 2, IsReviewed = true },
                    new CommentContent { Id = 3, IsReviewed = false },
                    new CommentContent { Id = 4, IsReviewed = true }
                }
            };
            var mappedDto = new CommentDTO
            {
                Id = commentId,
                Replies = new List<CommentDTO>
                {
                    new CommentDTO { Id = 3, IsReviewed = false }
                }
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(commentId))
                .ReturnsAsync(rootComment);
            _mapperMock.Setup(m => m.Map<CommentDTO>(rootComment))
                .Returns(mappedDto);

            var query = new GetCommentByIdQuery(commentId, false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(mappedDto, result.Value);

            _repositoryWrapperMock.Verify(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(commentId), Times.Once);
            _mapperMock.Verify(m => m.Map<CommentDTO>(rootComment), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithoutIsReviewedFilter_ReturnsAllReplies()
        {
            // Arrange
            var commentId = 1;
            var rootComment = new CommentContent
            {
                Id = commentId,
                Replies = new List<CommentContent>
                {
                    new CommentContent { Id = 2, IsReviewed = true },
                    new CommentContent { Id = 3, IsReviewed = false }
                }
            };
            var mappedDto = new CommentDTO
            {
                Id = commentId,
                Replies = new List<CommentDTO>
                {
                    new CommentDTO { Id = 2, IsReviewed = true },
                    new CommentDTO { Id = 3, IsReviewed = false }
                }
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(commentId))
                .ReturnsAsync(rootComment);
            _mapperMock.Setup(m => m.Map<CommentDTO>(rootComment))
                .Returns(mappedDto);

            var query = new GetCommentByIdQuery(commentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(mappedDto, result.Value);

            _repositoryWrapperMock.Verify(r => r.CommentRepository.GetCommentTreeByCommentIdAsync(commentId), Times.Once);
            _mapperMock.Verify(m => m.Map<CommentDTO>(rootComment), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}