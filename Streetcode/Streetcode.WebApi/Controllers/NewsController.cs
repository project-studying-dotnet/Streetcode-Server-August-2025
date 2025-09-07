using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.BLL.MediatR.Newss.GetById;
using Streetcode.BLL.MediatR.Newss.GetByUrl;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;

namespace Streetcode.WebApi.Controllers;

/// <summary>
/// Not finished controller created for testing purposes
/// in the future it will be worth refining and rechecking
/// </summary>
public class NewsController : BaseApiController
{
    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPost]
    [ProducesResponseType(typeof(NewsDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateNews([FromBody] NewsDTO newsDto, CancellationToken ct)
    {
        var command = new CreateNewsCommand(newsDto);
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NewsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllNews(CancellationToken ct)
    {
        var query = new GetAllNewsQuery();
        var result = await Mediator.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNewsById([FromRoute] int id)
    {
        var query = new GetNewsByIdQuery(id);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("url/{url}")]
    public async Task<IActionResult> GetNewsByUrl([FromRoute] string url)
    {
        var query = new GetNewsByUrlQuery(url);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPut]
    public async Task<IActionResult> UpdateNews([FromBody] NewsDTO newsDto)
    {
        var command = new UpdateNewsCommand(newsDto);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteNews([FromRoute] int id, CancellationToken ct)
    {
        var command = new DeleteNewsCommand(id);
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }
}
