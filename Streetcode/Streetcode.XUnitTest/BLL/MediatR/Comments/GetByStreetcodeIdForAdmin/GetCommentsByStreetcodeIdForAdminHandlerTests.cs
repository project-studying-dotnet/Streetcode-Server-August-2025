using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeIdForAdmin;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments.GetByStreetcodeIdForAdmin
{
    public class GetCommentsByStreetcodeIdForAdminHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetCommentsByStreetcodeIdForAdminHandler _handler;

        public GetCommentsByStreetcodeIdForAdminHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetCommentsByStreetcodeIdForAdminHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingComments_ReturnsSuccessAndMappedDtos()
        {
            // Arrange
            var streetcodeId = 1;
            var comments = new List<CommentContent>
            {
                new CommentContent { Id = 1, StreetcodeId = streetcodeId, IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = streetcodeId, IsDeleted = false },
            };
            var expectedDtos = new List<CommentDTO>
            {
                new CommentDTO { Id = 1 },
                new CommentDTO { Id = 2 },
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(comments);

            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDTO>>(comments))
                .Returns(expectedDtos);

            var query = new GetCommentsByStreetcodeIdForAdminQuery(streetcodeId, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDtos, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<CommentDTO>>(comments), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_CommentsDoNotExist_ReturnsFailAndLogsError()
        {
            // Arrange
            var streetcodeId = 999;
            var errorMsg = Errors_Common.NotFoundByStreetcode.FormatWith("comment", streetcodeId);
            var comments = new List<CommentContent>();

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(comments);

            var query = new GetCommentsByStreetcodeIdForAdminQuery(streetcodeId, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message == errorMsg);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);

            _loggerMock.Verify(l => l.LogError(query, errorMsg), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithIsReviewedFilterTrue_ReturnsFilteredComments()
        {
            // Arrange
            var streetcodeId = 1;
            var comments = new List<CommentContent>
            {
                new CommentContent { Id = 1, StreetcodeId = streetcodeId, IsRestricted = true, IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = streetcodeId, IsRestricted = null, IsDeleted = false },
            };
            var expectedDtos = new List<CommentDTO>
            {
                new CommentDTO { Id = 1, IsRestricted = true },
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(new List<CommentContent> { comments[0] });

            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDTO>>(It.IsAny<IEnumerable<CommentContent>>()))
                .Returns(expectedDtos);

            var query = new GetCommentsByStreetcodeIdForAdminQuery(streetcodeId, true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDtos, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<CommentDTO>>(It.IsAny<IEnumerable<CommentContent>>()), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithIsReviewedFilterFalse_ReturnsFilteredComments()
        {
            // Arrange
            var streetcodeId = 1;
            var comments = new List<CommentContent>
            {
                new CommentContent { Id = 1, StreetcodeId = streetcodeId, IsRestricted = true, IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = streetcodeId, IsRestricted = null, IsDeleted = false },
            };
            var expectedDtos = new List<CommentDTO>
            {
                new CommentDTO { Id = 2, IsRestricted = null },
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(new List<CommentContent> { comments[^1] });
            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDTO>>(It.IsAny<IEnumerable<CommentContent>>()))
                .Returns(expectedDtos);

            var query = new GetCommentsByStreetcodeIdForAdminQuery(streetcodeId, false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDtos, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<CommentDTO>>(It.IsAny<IEnumerable<CommentContent>>()), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithDeletedComment_ReturnsOnlyActiveComments()
        {
            // Arrange
            var streetcodeId = 1;
            var comments = new List<CommentContent>
            {
                new CommentContent { Id = 1, StreetcodeId = streetcodeId, IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = streetcodeId, IsDeleted = true },
            };
            var activeComments = new List<CommentContent>
            {
                new CommentContent { Id = 1, StreetcodeId = streetcodeId, IsDeleted = false },
            };
            var expectedDtos = new List<CommentDTO>
            {
                new CommentDTO { Id = 1 },
            };

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(activeComments);

            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDTO>>(activeComments))
                .Returns(expectedDtos);

            var query = new GetCommentsByStreetcodeIdForAdminQuery(streetcodeId, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDtos, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<CommentDTO>>(activeComments), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}