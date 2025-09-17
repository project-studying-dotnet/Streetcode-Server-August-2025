using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments.Update
{
    public class UpdateCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UpdateCommentHandler _handler;

        public UpdateCommentHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new UpdateCommentHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ReturnsSuccess()
        {
            // Arrange
            var requestDto = new CommentUpdateDTO { Id = 1, Text = "Updated text" };
            var existingComment = new CommentContent { Id = 1, Text = "Old text", UserId = 1 };
            var resultDto = new CommentDTO { Id = requestDto.Id, Text = requestDto.Text, UpdatedAt = DateTime.UtcNow };
            var command = new UpdateCommentCommand(requestDto, existingComment.UserId);

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(existingComment);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map(requestDto, existingComment));
            _mapperMock.Setup(m => m.Map<CommentDTO>(existingComment)).Returns(resultDto);
            _repositoryWrapperMock.Setup(r => r.CommentRepository.Update(existingComment));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(resultDto, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _repositoryWrapperMock.Verify(r => r.CommentRepository.Update(existingComment), Times.Once);
            _mapperMock.Verify(m => m.Map(requestDto, existingComment), Times.Once);
            _mapperMock.Verify(m => m.Map<CommentDTO>(existingComment), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenCommentNotFound_ReturnsFailAndLogsError()
        {
            // Arrange
            int userId = 1;
            var requestDto = new CommentUpdateDTO { Id = 1 };
            var command = new UpdateCommentCommand(requestDto, userId);
            string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", command.Comment.Id);

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync((CommentContent)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenUnauthorized_ReturnsFailAndLogsError()
        {
            // Arrange
            var requestDto = new CommentUpdateDTO { Id = 1, Text = "Updated text" };
            var existingComment = new CommentContent { Id = 1, Text = "Old text", UserId = 1 };
            var command = new UpdateCommentCommand(requestDto, 2);
            string errorMsg = Errors_Common.UnauthorizedAction.FormatWith("update this comment");

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(existingComment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenFailedToSave_ReturnsFailAndLogsError()
        {
            // Arrange
            var requestDto = new CommentUpdateDTO { Id = 1, Text = "Updated text" };
            var existingComment = new CommentContent { Id = 1, Text = "Old text", UserId = 1 };
            var command = new UpdateCommentCommand(requestDto, existingComment.UserId);
            string errorMsg = Errors_Common.FailedToUpdate.FormatWith("comment");

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ReturnsAsync(existingComment);

            _mapperMock.Setup(m => m.Map(requestDto, existingComment));
            _repositoryWrapperMock.Setup(r => r.CommentRepository.Update(existingComment));
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.CommentRepository.Update(existingComment), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map(requestDto, existingComment), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
            _loggerMock.Verify(l => l.LogError(command, errorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsException_ReturnsFail()
        {
            // Arrange
            int userId = 1;
            var requestDto = new CommentUpdateDTO { Id = 1, Text = "Updated text" };
            var command = new UpdateCommentCommand(requestDto, userId);
            const string errorMsg = "Database connection lost";

            _repositoryWrapperMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
                .ThrowsAsync(new System.Exception(errorMsg));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<UpdateCommentCommand>(), errorMsg), Times.Once);
            _repositoryWrapperMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
