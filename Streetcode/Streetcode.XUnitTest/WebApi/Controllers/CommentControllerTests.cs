using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.MediatR.Comments.Delete;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Comments.SetCommentRestrictedStatus;
using Streetcode.BLL.MediatR.Comments.Update;
using Streetcode.WebApi.Controllers.Comments;
using Xunit;
using FluentResults;

namespace Streetcode.XUnitTest.WebApi.Controllers;

public class CommentControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly CommentController _controller;

    public CommentControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        _controller = new CommentController();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = _serviceProviderMock.Object;

        // Provide default authenticated user with id and role claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Moderator")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

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
                "GetByStreetcodeId",
                new Func<CommentController, Task<IActionResult>>(async controller =>
                    await controller.GetByStreetcodeId(1)),
                typeof(GetCommentsByStreetcodeIdQuery)
            },
            new object[]
            {
                "Create",
                new Func<CommentController, Task<IActionResult>>(async controller =>
                    await controller.Create(new CommentCreateDTO())),
                typeof(CreateCommentCommand)
            },
            new object[]
            {
                "Update",
                new Func<CommentController, Task<IActionResult>>(async controller =>
                    await controller.Update(new CommentUpdateDTO())),
                typeof(UpdateCommentCommand)
            },
            new object[]
            {
                "DeleteComment",
                new Func<CommentController, Task<IActionResult>>(async controller =>
                    await controller.DeleteComment(1, CancellationToken.None)),
                typeof(DeleteCommentCommand)
            },
            new object[]
            {
                "ApproveComment",
                new Func<CommentController, Task<IActionResult>>(async controller =>
                    await controller.ApproveComment(1, CancellationToken.None)),
                typeof(SetCommentRestrictedStatusCommand)
            },
            new object[]
            {
                "RestrictComment",
                new Func<CommentController, Task<IActionResult>>(async controller =>
                    await controller.RestrictComment(1, CancellationToken.None)),
                typeof(SetCommentRestrictedStatusCommand)
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetControllerMethodsData))]
    public async Task ControllerMethods_ShouldCallMediatorAndReturnOk_WhenResultIsSuccess(
        string methodName,
        Func<CommentController, Task<IActionResult>> methodCall,
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
        Func<CommentController, Task<IActionResult>> methodCall,
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
            case not null when requestType == typeof(CreateCommentCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new CommentDTO())
                        : Result.Fail<CommentDTO>("Test error"));
                break;

            case not null when requestType == typeof(UpdateCommentCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new CommentDTO())
                        : Result.Fail<CommentDTO>("Test error"));
                break;

            case not null when requestType == typeof(GetCommentsByStreetcodeIdQuery):
                _mediatorMock.Setup(x => x.Send(It.IsAny<GetCommentsByStreetcodeIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(Enumerable.Empty<CommentDTO>())
                        : Result.Fail<IEnumerable<CommentDTO>>("Test error"));
                break;

            case not null when requestType == typeof(DeleteCommentCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new CommentDTO())
                        : Result.Fail<CommentDTO>("Test error"));
                break;

            case not null when requestType == typeof(SetCommentRestrictedStatusCommand):
                _mediatorMock.Setup(x => x.Send(It.IsAny<SetCommentRestrictedStatusCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isSuccess
                        ? Result.Ok(new CommentDTO())
                        : Result.Fail<CommentDTO>("Test error"));
                break;
        }
    }
}