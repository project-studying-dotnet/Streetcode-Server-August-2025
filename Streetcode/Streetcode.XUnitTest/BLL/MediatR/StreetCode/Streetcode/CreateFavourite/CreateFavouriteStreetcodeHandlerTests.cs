using System.Linq.Expressions;
using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.CreateFavourite;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Entities.Favourite;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.FavouriteStreetcodes;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Streetcode.CreateFavourite;

public class CreateFavouriteStreetcodeHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IFavouriteStreetcodeRepository> _mockFavouriteRepository;
    private readonly CreateFavouriteStreetcodeHandler _handler;

    public CreateFavouriteStreetcodeHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockFavouriteRepository = new Mock<IFavouriteStreetcodeRepository>();

        _mockRepositoryWrapper.Setup(x => x.FavoriteStreetcodeRepository)
            .Returns(_mockFavouriteRepository.Object);

        _handler = new CreateFavouriteStreetcodeHandler(
            _mockRepositoryWrapper.Object,
            _mockLogger.Object,
            _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndNoExistingFavourite_ShouldCreateFavouriteSuccessfully()
    {
        var userId = 123;
        var streetcodeId = 456;
        var command = new CreateFavouriteStreetcodeCommand(streetcodeId);

        SetupHttpContextWithValidUser(userId);
        _mockFavouriteRepository.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<FavouriteStreetcode, bool>>>(),
            It.IsAny<Func<IQueryable<FavouriteStreetcode>, IIncludableQueryable<FavouriteStreetcode, object>>>()))
            .ReturnsAsync((FavouriteStreetcode?)null);
        _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _mockFavouriteRepository.Verify(
            x => x.CreateAsync(It.Is<FavouriteStreetcode>(f =>
                f.UserId == userId && f.StreetcodeId == streetcodeId)),
            Times.Once);
        _mockRepositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHttpContextIsNull_ShouldReturnFailureWithUserNotFoundError()
    {
        var command = new CreateFavouriteStreetcodeCommand(123);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(Errors_Jwt.UserNotFound);
        _mockLogger.Verify(x => x.LogError(command, Errors_Jwt.UserNotFound), Times.Once);
        _mockFavouriteRepository.Verify(x => x.CreateAsync(It.IsAny<FavouriteStreetcode>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserClaimsAreNull_ShouldThrowArgumentNullException()
    {
        var command = new CreateFavouriteStreetcodeCommand(123);
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User.Claims).Returns((IEnumerable<Claim>?)null);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(command, CancellationToken.None));

        _mockLogger.Verify(x => x.LogError(command, It.IsAny<string>()), Times.Never);
        _mockFavouriteRepository.Verify(x => x.CreateAsync(It.IsAny<FavouriteStreetcode>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdClaimNotFound_ShouldReturnFailureWithUserNotFoundError()
    {
        var command = new CreateFavouriteStreetcodeCommand(123);
        var claims = new List<Claim> { new Claim("SomeOtherClaim", "value") };
        SetupHttpContextWithClaims(claims);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(Errors_Jwt.UserNotFound);
        _mockLogger.Verify(x => x.LogError(command, Errors_Jwt.UserNotFound), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdClaimIsEmpty_ShouldReturnFailureWithUserNotFoundError()
    {
        var command = new CreateFavouriteStreetcodeCommand(123);
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "") };
        SetupHttpContextWithClaims(claims);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(Errors_Jwt.UserNotFound);
        _mockLogger.Verify(x => x.LogError(command, Errors_Jwt.UserNotFound), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFavouriteAlreadyExists_ShouldReturnFailureWithAlreadyExistsError()
    {
        var userId = 123;
        var streetcodeId = 456;
        var command = new CreateFavouriteStreetcodeCommand(streetcodeId);
        var existingFavourite = new FavouriteStreetcode { UserId = userId, StreetcodeId = streetcodeId };

        SetupHttpContextWithValidUser(userId);
        _mockFavouriteRepository.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<FavouriteStreetcode, bool>>>(),
            It.IsAny<Func<IQueryable<FavouriteStreetcode>, IIncludableQueryable<FavouriteStreetcode, object>>>()))
            .ReturnsAsync(existingFavourite);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Favourite streetcode");
        result.Errors.First().Message.Should().Contain("already exists");
        _mockFavouriteRepository.Verify(x => x.CreateAsync(It.IsAny<FavouriteStreetcode>()), Times.Never);
        _mockRepositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailureWithFailedToCreateError()
    {
        var userId = 123;
        var streetcodeId = 456;
        var command = new CreateFavouriteStreetcodeCommand(streetcodeId);

        SetupHttpContextWithValidUser(userId);
        _mockFavouriteRepository.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<FavouriteStreetcode, bool>>>(),
            It.IsAny<Func<IQueryable<FavouriteStreetcode>, IIncludableQueryable<FavouriteStreetcode, object>>>()))
            .ReturnsAsync((FavouriteStreetcode?)null);
        _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("favourite streetcode");
        result.Errors.First().Message.Should().Contain("Failed to create");
        _mockFavouriteRepository.Verify(x => x.CreateAsync(It.IsAny<FavouriteStreetcode>()), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Handle_WhenSaveChangesReturnsNonPositiveValue_ShouldReturnFailure(int saveResult)
    {
        var userId = 123;
        var streetcodeId = 456;
        var command = new CreateFavouriteStreetcodeCommand(streetcodeId);

        SetupHttpContextWithValidUser(userId);
        _mockFavouriteRepository.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<FavouriteStreetcode, bool>>>(),
            It.IsAny<Func<IQueryable<FavouriteStreetcode>, IIncludableQueryable<FavouriteStreetcode, object>>>()))
            .ReturnsAsync((FavouriteStreetcode?)null);
        _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(saveResult);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Failed to create");
    }

    [Fact]
    public async Task Handle_ShouldCheckForExistingFavouriteWithCorrectParameters()
    {
        var userId = 123;
        var streetcodeId = 456;
        var command = new CreateFavouriteStreetcodeCommand(streetcodeId);

        SetupHttpContextWithValidUser(userId);
        _mockFavouriteRepository.Setup(x => x.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<FavouriteStreetcode, bool>>>(),
            It.IsAny<Func<IQueryable<FavouriteStreetcode>, IIncludableQueryable<FavouriteStreetcode, object>>>()))
            .ReturnsAsync((FavouriteStreetcode?)null);
        _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _mockFavouriteRepository.Verify(
            x => x.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<FavouriteStreetcode, bool>>>(expr =>
                    expr.Compile()(new FavouriteStreetcode { StreetcodeId = streetcodeId, UserId = userId })),
                It.IsAny<Func<IQueryable<FavouriteStreetcode>, IIncludableQueryable<FavouriteStreetcode, object>>>()),
            Times.Once);
    }

    private void SetupHttpContextWithValidUser(int userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        SetupHttpContextWithClaims(claims);
    }

    private void SetupHttpContextWithClaims(IEnumerable<Claim> claims)
    {
        var mockHttpContext = new Mock<HttpContext>();
        var mockUser = new Mock<ClaimsPrincipal>();
        mockUser.Setup(x => x.Claims).Returns(claims);
        mockHttpContext.Setup(x => x.User).Returns(mockUser.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
    }
}
