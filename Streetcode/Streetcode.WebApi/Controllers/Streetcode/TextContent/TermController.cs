using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Streetcode.BLL.MediatR.Streetcode.Term.Delete;
using Streetcode.BLL.MediatR.Streetcode.Term.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Term.GetById;
using Streetcode.BLL.MediatR.Streetcode.Term.Update;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;
public class TermController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllTermsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetTermByIdQuery(id)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTermDTO termDto)
    {
        return HandleResult(await Mediator.Send(new CreateTermCommand(termDto)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CreateTermDTO termDto)
    {
        return HandleResult(await Mediator.Send(new UpdateTermCommand(id, termDto)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteTermCommand(id)));
    }
}
