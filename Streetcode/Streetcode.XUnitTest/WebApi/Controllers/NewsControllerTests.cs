using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.BLL.MediatR.Newss.GetById;
using Streetcode.BLL.MediatR.Newss.GetByUrl;
using Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl;
using Streetcode.BLL.MediatR.Newss.SortedByDateTime;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.WebApi.Controllers;
using Xunit;
using FluentResults;

namespace Streetcode.XUnitTest.WebApi.Controllers;

public class NewsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly NewsController _controller;

    public NewsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        _controller = new NewsController();

        // Setup HttpContext to mock the service provider
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = _serviceProviderMock.Object;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    public static IEnumerable<object[]> GetControllerMethodsData()
    {
        return new List<object[]>
        {
            new object[]
            {
                "CreateNews",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.CreateNews(new NewsDTO(), CancellationToken.None)),
                typeof(CreateNewsCommand)
            },
            new object[]
            {
                "GetAllNews",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.GetAllNews(CancellationToken.None)),
                typeof(GetAllNewsQuery)
            },
            new object[]
            {
                "GetNewsById",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.GetNewsById(1, CancellationToken.None)),
                typeof(GetNewsByIdQuery)
            },
            new object[]
            {
                "GetNewsByUrl",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.GetNewsByUrl("test-url", CancellationToken.None)),
                typeof(GetNewsByUrlQuery)
            },
            new object[]
            {
                "UpdateNews",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.UpdateNews(new NewsDTO(), CancellationToken.None)),
                typeof(UpdateNewsCommand)
            },
            new object[]
            {
                "DeleteNews",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.DeleteNews(1, CancellationToken.None)),
                typeof(DeleteNewsCommand)
            },
            new object[]
            {
                "GetNewsAndLinksByUrl",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.GetNewsAndLinksByUrl("test-url", CancellationToken.None)),
                typeof(GetNewsAndLinksByUrlQuery)
            },
            new object[]
            {
                "GetNewsSortedByDate",
                new Func<NewsController, Task<IActionResult>>(async controller =>
                    await controller.GetNewsSortedByDate(CancellationToken.None)),
                typeof(SortedByDateTimeQuery)
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetControllerMethodsData))]
    public async Task ControllerMethods_ShouldCallMediatorAndReturnOk_WhenResultIsSuccess(
        string methodName,
        Func<NewsController, Task<IActionResult>> methodCall,
        Type expectedRequestType)
    {
        // Arrange
        SetupMediatorMockForType(expectedRequestType, true);

        // Act
        var result = await methodCall(_controller);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.VerifyAll();
    }

    [Theory]
    [MemberData(nameof(GetControllerMethodsData))]
    public async Task ControllerMethods_ShouldReturnBadRequest_WhenResultIsFailure(
        string methodName,
        Func<NewsController, Task<IActionResult>> methodCall,
        Type expectedRequestType)
    {
        // Arrange
        SetupMediatorMockForType(expectedRequestType, false);

        // Act
        var result = await methodCall(_controller);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mediatorMock.VerifyAll();
    }

    private void SetupMediatorMockForType(Type requestType, bool isSuccess)
    {
        switch (requestType)
        {
            case not null when requestType == typeof(CreateNewsCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<CreateNewsCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new NewsDTO())
                        : Result.Fail<NewsDTO>("Test error"));
                break;

            case not null when requestType == typeof(UpdateNewsCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateNewsCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new NewsDTO())
                        : Result.Fail<NewsDTO>("Test error"));
                break;

            case not null when requestType == typeof(GetAllNewsQuery):
                _mediatorMock.Setup(x => x.Send(It.IsAny<GetAllNewsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(Enumerable.Empty<NewsDTO>())
                        : Result.Fail<IEnumerable<NewsDTO>>("Test error"));
                break;

            case not null when requestType == typeof(GetNewsByIdQuery):
                _mediatorMock.Setup(x => x.Send(It.IsAny<GetNewsByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new NewsDTO())
                        : Result.Fail<NewsDTO>("Test error"));
                break;

            case not null when requestType == typeof(GetNewsByUrlQuery):
                _mediatorMock.Setup(x => x.Send(It.IsAny<GetNewsByUrlQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new NewsDTO())
                        : Result.Fail<NewsDTO>("Test error"));
                break;

            case not null when requestType == typeof(DeleteNewsCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteNewsCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok()
                        : Result.Fail("Test error"));
                break;

            case not null when requestType == typeof(GetNewsAndLinksByUrlQuery):
                _mediatorMock.Setup(x => x.Send(It.IsAny<GetNewsAndLinksByUrlQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new NewsDTOWithURLs())
                        : Result.Fail<NewsDTOWithURLs>("Test error"));
                break;

            case not null when requestType == typeof(SortedByDateTimeQuery):
                _mediatorMock.Setup(x => x.Send(It.IsAny<SortedByDateTimeQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new List<NewsDTO> { new() })
                        : Result.Fail<List<NewsDTO>>("Test error"));
                break;
        }
    }
}