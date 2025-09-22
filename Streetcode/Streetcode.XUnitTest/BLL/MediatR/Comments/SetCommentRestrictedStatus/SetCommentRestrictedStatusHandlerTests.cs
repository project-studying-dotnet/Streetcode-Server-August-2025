using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.SetCommentRestrictedStatus;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments.SetCommentRestrictedStatus;

public class SetCommentRestrictedStatusHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly SetCommentRestrictedStatusHandler _handler;

    public SetCommentRestrictedStatusHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new SetCommentRestrictedStatusHandler(
            _repositoryWrapperMock.Object,
            _loggerMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_CommentNotFound_ReturnsFailureResult()
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, true);
        var cancellationToken = CancellationToken.None;

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync((CommentContent?)null);

        string expectedErrorMsg = Errors_Common.NotFoundById.FormatWith("comment", command.CommentId);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(expectedErrorMsg, result.Errors.Select(e => e.Message));

        _loggerMock.Verify(
            x => x.LogError(command, expectedErrorMsg),
            Times.Once);
    }

    public async Task Handle_CommentAlreadyHasSameRestrictedStatusAndIsReviewed_ReturnsSuccessWithoutUpdate()
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, true);
        var cancellationToken = CancellationToken.None;

        var comment = new CommentContent
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = true, // Same as command
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var expectedCommentDto = new CommentDTO
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = true
        };

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        _mapperMock
            .Setup(x => x.Map<CommentDTO>(comment))
            .Returns(expectedCommentDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCommentDto, result.Value);

        // Verify that Update and SaveChanges were not called
        _repositoryWrapperMock.Verify(x => x.CommentRepository.Update(It.IsAny<CommentContent>()), Times.Never);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_CommentNotReviewedYet_UpdatesSuccessfully()
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, true);
        var cancellationToken = CancellationToken.None;

        var comment = new CommentContent
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = null, // Not reviewed yet
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var expectedCommentDto = new CommentDTO
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = true
        };

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<CommentDTO>(comment))
            .Returns(expectedCommentDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCommentDto, result.Value);
        Assert.Equal(true, comment.IsRestricted);

        _repositoryWrapperMock.Verify(x => x.CommentRepository.Update(comment), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CommentHasDifferentRestrictedStatus_UpdatesSuccessfully()
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, false);
        var cancellationToken = CancellationToken.None;

        var comment = new CommentContent
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = true, // Different from command
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var expectedCommentDto = new CommentDTO
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = false
        };

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<CommentDTO>(comment))
            .Returns(expectedCommentDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCommentDto, result.Value);
        Assert.Equal(false, comment.IsRestricted);

        _repositoryWrapperMock.Verify(x => x.CommentRepository.Update(comment), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ReturnsFailureResult()
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, true);
        var cancellationToken = CancellationToken.None;

        var comment = new CommentContent
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = false,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0); // No rows affected

        string expectedErrorMsg = Errors_Common.FailedToUpdate.FormatWith("comment");

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(expectedErrorMsg, result.Errors.Select(e => e.Message));

        _repositoryWrapperMock.Verify(x => x.CommentRepository.Update(comment), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        _loggerMock.Verify(
            x => x.LogError(command, expectedErrorMsg),
            Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ValidRequest_SetsCorrectRestrictedValue(bool isRestricted)
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, isRestricted);
        var cancellationToken = CancellationToken.None;

        var comment = new CommentContent
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = !isRestricted, // Opposite value
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var expectedCommentDto = new CommentDTO
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = isRestricted
        };

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<CommentDTO>(comment))
            .Returns(expectedCommentDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(isRestricted, comment.IsRestricted);
        Assert.Equal(expectedCommentDto, result.Value);

        _repositoryWrapperMock.Verify(x => x.CommentRepository.Update(comment), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesTimestampCorrectly()
    {
        // Arrange
        var command = new SetCommentRestrictedStatusCommand(1, true);
        var cancellationToken = CancellationToken.None;

        var comment = new CommentContent
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = false,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
        };

        var expectedCommentDto = new CommentDTO
        {
            Id = 1,
            Text = "Test comment",
            IsRestricted = true
        };

        _repositoryWrapperMock
            .Setup(x => x.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentContent, bool>>>(),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<CommentDTO>(comment))
            .Returns(expectedCommentDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCommentDto, result.Value);
    }
}