using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.FavouriteStreetcode.GetFavoritesByUserId;
using Streetcode.DAL.Entities.Favourite;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.FavouriteStreetcode.GetFavoritesByUserId;

public class GetFavoritesByUserIdQueryHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetFavoritesByUserIdHandler _handler;

    public GetFavoritesByUserIdQueryHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();

        _handler = new GetFavoritesByUserIdHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasFavoriteStreetcodes_ReturnsCorrectDTOs()
    {
        // Arrange
        var userId = 1;
        var query = new GetFavoritesByUserIdQuery(userId);

        var favoriteStreetcodes = new List<DAL.Entities.Favourite.FavouriteStreetcode>
        {
            new DAL.Entities.Favourite.FavouriteStreetcode { StreetcodeId = 1, UserId = 1 },
            new DAL.Entities.Favourite.FavouriteStreetcode { StreetcodeId = 2, UserId = 1 },
        };

        var streetcodeContents = new List<StreetcodeContent>
        {
            new StreetcodeContent { Id = 1, Title = "Streetcode 1" },
            new StreetcodeContent { Id = 2, Title = "Streetcode 2" },
        };

        var streetcodeDtos = new List<StreetcodeDTO>
        {
            new StreetcodeDTO { Id = 1, Title = "Streetcode 1" },
            new StreetcodeDTO { Id = 2, Title = "Streetcode 2" },
        };

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favoriteStreetcodes);

        _repositoryWrapperMock
            .Setup(x => x.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(streetcodeContents);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<StreetcodeDTO>>(It.IsAny<IEnumerable<StreetcodeContent>>()))
            .Returns(streetcodeDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal("Streetcode 1", result.Value.First().Title);
        Assert.Equal("Streetcode 2", result.Value.Last().Title);
    }

    [Fact]
    public async Task Handle_UserHasNoFavoriteStreetcodes_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var query = new GetFavoritesByUserIdQuery(userId);

        var favoriteStreetcodes = new List<DAL.Entities.Favourite.FavouriteStreetcode>();

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync(favoriteStreetcodes);

        _repositoryWrapperMock
            .Setup(x => x.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(new List<StreetcodeContent>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var query = new GetFavoritesByUserIdQuery(userId);

        _repositoryWrapperMock
            .Setup(x => x.FavouriteStreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Favourite.FavouriteStreetcode, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Favourite.FavouriteStreetcode>, IIncludableQueryable<DAL.Entities.Favourite.FavouriteStreetcode, object>>>()))
            .ReturnsAsync((IEnumerable<DAL.Entities.Favourite.FavouriteStreetcode>)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}