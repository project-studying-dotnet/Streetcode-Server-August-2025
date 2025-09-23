using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Streetcode.BLL.DTO.FavouriteStreetcode;
using Streetcode.BLL.MediatR.FavouriteStreetcode.Delete;
using Streetcode.WebApi.Controllers.FavouriteStreetcode;
using Xunit;
using FluentResults;

namespace Streetcode.XUnitTest.WebApi.Controllers;

public class FavouriteStreetcodeControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly FavouriteStreetcodeController _controller;

    public FavouriteStreetcodeControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        _controller = new FavouriteStreetcodeController();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = _serviceProviderMock.Object;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task DeleteFavouriteStreetcode_ShouldReturnOk_WhenResultIsSuccess()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DeleteFavouriteStreetcodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new FavouriteStreetcodeDTO()));

        // Act
        var result = await _controller.DeleteFavouriteStreetcode(1, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteFavouriteStreetcode_ShouldReturnBadRequest_WhenResultIsFailure()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DeleteFavouriteStreetcodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<FavouriteStreetcodeDTO>("Test error"));

        // Act
        var result = await _controller.DeleteFavouriteStreetcode(1, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mediatorMock.VerifyAll();
    }
}