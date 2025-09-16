using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;

namespace Streetcode.WebApi.Controllers.Comments;

public class CommentController : BaseApiController
{
    [HttpPost]
    [AuthorizeRoles]
    public async Task<IActionResult> Create([FromBody] CommentCreateDTO commentCreate)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreate)));
    }
}
