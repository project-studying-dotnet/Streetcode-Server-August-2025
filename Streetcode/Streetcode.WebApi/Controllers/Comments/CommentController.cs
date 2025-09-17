using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.MediatR.Comments.Delete;
using Streetcode.WebApi.Attributes;
using Streetcode.BLL.MediatR.Comments.Update;
using Streetcode.WebApi.Utils;

namespace Streetcode.WebApi.Controllers.Comments;

public class CommentController : BaseApiController
{
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
