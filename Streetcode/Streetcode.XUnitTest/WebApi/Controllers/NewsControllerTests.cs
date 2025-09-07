using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Streetcode.WebApi.Controllers;

namespace Streetcode.XUnitTest.WebApi.Controllers;

public class NewsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NewsController _controller;

    public NewsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();

        var httpContext = new DefaultHttpContext();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        httpContext.RequestServices = serviceProvider.Object;

        _controller = new NewsController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

}