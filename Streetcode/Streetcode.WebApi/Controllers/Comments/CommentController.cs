using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;
using Streetcode.BLL.MediatR.Comments.Update;

namespace Streetcode.WebApi.Controllers.Comments;

public class CommentController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    [AuthorizeRoles]
    public async Task<IActionResult> Create([FromBody] CommentCreateDTO commentCreate)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreate)));
    }

    [HttpPut]
    [AuthorizeRoles]
    public async Task<IActionResult> Update([FromBody] CommentUpdateDTO comment)
    {
        return HandleResult(await Mediator.Send(new UpdateCommentCommand(comment)));
    }
}
