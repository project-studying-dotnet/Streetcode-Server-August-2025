using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.MediatR.Comments.Delete;
using Streetcode.BLL.MediatR.Comments.GetAll;
using Streetcode.BLL.MediatR.Comments.GetById;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeIdForAdmin;
using Streetcode.BLL.MediatR.Comments.Update;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;
using Streetcode.WebApi.Utils;

namespace Streetcode.WebApi.Controllers.Comments;

public class CommentController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpGet("{streetcodeId:int}")]
    [AuthorizeRoles(UserRole.Moderator, UserRole.Administrator, UserRole.MainAdministrator)]

    public async Task<IActionResult> GetByStreetcodeIdForModeration(int streetcodeId, [FromQuery] bool? isReviewed)
    {
        return HandleResult(await Mediator.Send(
            new GetCommentsByStreetcodeIdForAdminQuery(streetcodeId, isReviewed)));
    }

    [HttpGet("{commentId:int}")]
    [AuthorizeRoles(UserRole.Moderator, UserRole.Administrator, UserRole.MainAdministrator)]
    public async Task<IActionResult> GetCommentById(int commentId, [FromQuery] bool? isReviewed)
    {
        return HandleResult(await Mediator.Send(
            new GetCommentByIdQuery(commentId, isReviewed)));
    }

    [HttpGet]
    [AuthorizeRoles(UserRole.Moderator, UserRole.Administrator, UserRole.MainAdministrator)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isReviewed = null)
    {
        return HandleResult(await Mediator.Send(new GetAllCommentsForAdminQuery(isReviewed)));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CommentCreateDTO commentCreate)
    {
        var userId = AuthHelper.GetUserId(HttpContext.User);
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreate, userId)));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteComment([FromRoute] int id, CancellationToken ct)
    {
        var userId = AuthHelper.GetUserId(HttpContext.User);
        var userRole = AuthHelper.GetUserRole(HttpContext.User);

        var command = new DeleteCommentCommand(id, userId, userRole);
        var result = await Mediator.Send(command, ct);

        return HandleResult(result);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] CommentUpdateDTO comment)
    {
        var userId = AuthHelper.GetUserId(HttpContext.User);
        return HandleResult(await Mediator.Send(new UpdateCommentCommand(comment, userId)));
    }
}
