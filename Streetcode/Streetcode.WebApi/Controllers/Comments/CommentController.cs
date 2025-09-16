using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Update;

namespace Streetcode.WebApi.Controllers.Comments;

public class CommentController : BaseApiController
{
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CommentUpdateDTO comment)
    {
        return HandleResult(await Mediator.Send(new UpdateCommentCommand(comment)));
    }
}
