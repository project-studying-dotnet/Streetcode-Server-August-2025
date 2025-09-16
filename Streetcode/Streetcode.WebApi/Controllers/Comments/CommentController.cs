using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.MediatR.Comments.Delete;
using Streetcode.WebApi.Attributes;
using Streetcode.WebApi.Utils;

namespace Streetcode.WebApi.Controllers.Comments;

public class CommentController : BaseApiController
{
    [HttpPost]
    [AuthorizeRoles]
    public async Task<IActionResult> Create([FromBody] CommentCreateDTO commentCreate)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreate)));
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
}
