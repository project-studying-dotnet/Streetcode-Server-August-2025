using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.FavouriteStreetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.FavouriteStreetcode.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.FavouriteStreetcode;

public class DeleteFavouriteStreetcodeTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly DeleteFavouriteStreetcodeHandler _handler;

    public DeleteFavouriteStreetcodeTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new DeleteFavouriteStreetcodeHandler(
            _repositoryWrapperMock.Object,
            _loggerMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_FavouriteNotFound_ReturnsFailureResult()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync((DAL.Entities.Favourite.FavouriteStreetcode?)null);

        string expectedErrorMsg = Errors_Common.NotFoundById.FormatWith("favourite streetcode", command.Id);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(expectedErrorMsg, result.Errors.Select(e => e.Message));

        _loggerMock.Verify(
            x => x.LogError(command, expectedErrorMsg),
            Times.Once);
    }

    public async Task Handle_UnauthorizedUser_ReturnsFailureResult()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        var favourite = new DAL.Entities.Favourite.FavouriteStreetcode
        {
            Id = 1,
            UserId = 456, // Different from requesting user
            StreetcodeId = 789
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favourite);

        string expectedErrorMsg = Errors_Common.UnauthorizedAction.FormatWith("delete this favourite streetcode");

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(expectedErrorMsg, result.Errors.Select(e => e.Message));

        _loggerMock.Verify(
            x => x.LogError(command, expectedErrorMsg),
            Times.Once);

        // Verify that Delete and SaveChanges were not called
        _repositoryWrapperMock.Verify(x => x.FavouriteStreetcodeRepository.Delete(It.IsAny<DAL.Entities.Favourite.FavouriteStreetcode>()), Times.Never);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_AuthorizedUser_DeletesSuccessfully()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        var favourite = new DAL.Entities.Favourite.FavouriteStreetcode
        {
            Id = 1,
            UserId = 123, // Same as requesting user
            StreetcodeId = 789
        };

        var expectedFavouriteDto = new FavouriteStreetcodeDTO
        {
            Id = 1,
            UserId = 123,
            StreetcodeId = 789
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favourite);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<FavouriteStreetcodeDTO>(favourite))
            .Returns(expectedFavouriteDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFavouriteDto, result.Value);

        _repositoryWrapperMock.Verify(x => x.FavouriteStreetcodeRepository.Delete(favourite), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ReturnsFailureResult()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        var favourite = new DAL.Entities.Favourite.FavouriteStreetcode
        {
            Id = 1,
            UserId = 123, // Same as requesting user
            StreetcodeId = 789
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favourite);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0); // No rows affected

        string expectedErrorMsg = Errors_Common.FailedToDelete.FormatWith("favourite streetcode");

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(expectedErrorMsg, result.Errors.Select(e => e.Message));

        _repositoryWrapperMock.Verify(x => x.FavouriteStreetcodeRepository.Delete(favourite), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        _loggerMock.Verify(
            x => x.LogError(command, expectedErrorMsg),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(5, 200)]
    [InlineData(10, 300)]
    public async Task Handle_ValidRequestWithDifferentIds_DeletesSuccessfully(int favouriteId, int userId)
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(favouriteId, userId);
        var cancellationToken = CancellationToken.None;

        var favourite = new DAL.Entities.Favourite.FavouriteStreetcode
        {
            Id = favouriteId,
            UserId = userId, // Same as requesting user
            StreetcodeId = 789
        };

        var expectedFavouriteDto = new FavouriteStreetcodeDTO
        {
            Id = favouriteId,
            UserId = userId,
            StreetcodeId = 789
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favourite);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<FavouriteStreetcodeDTO>(favourite))
            .Returns(expectedFavouriteDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFavouriteDto, result.Value);
        Assert.Equal(favouriteId, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);

        _repositoryWrapperMock.Verify(x => x.FavouriteStreetcodeRepository.Delete(favourite), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesReturnsNegativeValue_ReturnsFailureResult()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        var favourite = new DAL.Entities.Favourite.FavouriteStreetcode
        {
            Id = 1,
            UserId = 123,
            StreetcodeId = 789
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favourite);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(-1); // Negative value indicating failure

        string expectedErrorMsg = Errors_Common.FailedToDelete.FormatWith("favourite streetcode");

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(expectedErrorMsg, result.Errors.Select(e => e.Message));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_MapperReturnsNull_ReturnsSuccessWithNull()
    {
        // Arrange
        var command = new DeleteFavouriteStreetcodeCommand(1, 123);
        var cancellationToken = CancellationToken.None;

        var favourite = new DAL.Entities.Favourite.FavouriteStreetcode
        {
            Id = 1,
            UserId = 123,
            StreetcodeId = 789
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favourite);

        _repositoryWrapperMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<FavouriteStreetcodeDTO>(favourite))
            .Returns((FavouriteStreetcodeDTO?)null);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);

        _repositoryWrapperMock.Verify(x => x.FavouriteStreetcodeRepository.Delete(favourite), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}