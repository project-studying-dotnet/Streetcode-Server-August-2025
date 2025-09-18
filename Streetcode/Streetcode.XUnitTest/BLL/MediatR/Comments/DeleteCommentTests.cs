using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.Delete;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Comments;
using Streetcode.BLL.Util.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments;

public class DeleteCommentTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly DeleteCommentCommandHandler _handler;

    public DeleteCommentTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockLogger = new Mock<ILoggerService>();
        _mockMapper = new Mock<IMapper>();

        _mockRepositoryWrapper.Setup(r => r.CommentRepository)
            .Returns(_mockCommentRepository.Object);

        _handler = new DeleteCommentCommandHandler(
            _mockRepositoryWrapper.Object,
            _mockLogger.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task DeleteComment_WhenAdminOrOwnerAndNoReplies_ShouldHardDelete()
    {
        // Arrange
        var command = CreateDeleteCommand(1, 100, UserRole.Administrator);
        var comment = CreateComment(1, 100);
        var mappedComment = new CommentDTO { Id = 1, UserId = 100, Text = "Test", CreatedAt = DateTime.UtcNow, StreetcodeId = 1 };

        _mockCommentRepository.Setup(r => r.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<CommentContent, bool>>>(),
            It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<CommentDTO>(comment)).Returns(mappedComment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(mappedComment);
        _mockCommentRepository.Verify(r => r.Delete(comment), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_WhenAdminOrOwnerAndHasReplies_ShouldSoftDelete()
    {
        // Arrange
        var command = CreateDeleteCommand(1, 100, UserRole.Administrator);
        var comment = CreateComment(1, 100);
        comment.Replies = new List<CommentContent>
        {
            new()
            {
                Id = 2
            }
        };
        var mappedComment = new CommentDTO
        {
            Id = 1,
            UserId = 100,
            Text = "Test",
            CreatedAt = DateTime.UtcNow,
            StreetcodeId = 1,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };

        _mockCommentRepository.Setup(r => r.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<CommentContent, bool>>>(),
            It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<CommentDTO>(comment)).Returns(mappedComment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(mappedComment);
        _mockCommentRepository.Verify(r => r.Delete(It.IsAny<CommentContent>()), Times.Never);
        _mockCommentRepository.Verify(r => r.Update(comment), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        comment.IsDeleted.Should().BeTrue();
        comment.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteComment_WhenUnauthorized_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateDeleteCommand(1, 200, UserRole.User); // Not owner, not admin
        var comment = CreateComment(1, 100); // Owner is 100
        var errorMsg = Errors_Common.UnauthorizedAction.FormatWith("delete this comment");

        _mockCommentRepository.Setup(r => r.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<CommentContent, bool>>>(),
            It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);
        _mockCommentRepository.Verify(r => r.Delete(It.IsAny<CommentContent>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteComment_WhenCommentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateDeleteCommand(1, 100, UserRole.User);
        var errorMsg = Errors_Common.NotFoundById.FormatWith("comment", command.Id);

        _mockCommentRepository.Setup(r => r.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<CommentContent, bool>>>(),
            It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync((CommentContent?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);
        _mockCommentRepository.Verify(r => r.Delete(It.IsAny<CommentContent>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteComment_WhenSaveChangesReturnsZero_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateDeleteCommand(1, 100, UserRole.MainAdministrator);
        var comment = CreateComment(1, 100);
        var errorMsg = Errors_Common.FailedToDelete.FormatWith("comment");

        _mockCommentRepository.Setup(r => r.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<CommentContent, bool>>>(),
            It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);
        _mockCommentRepository.Verify(r => r.Delete(comment), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // Helper methods
    private static DeleteCommentCommand CreateDeleteCommand(int id, int requestingUserId, UserRole role)
        => new(id, requestingUserId, role);

    private static CommentContent CreateComment(int id, int userId)
        => new CommentContent
        {
            Id = id,
            Text = "Test",
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            StreetcodeId = 1,
            Replies = new List<CommentContent>()
        };
}