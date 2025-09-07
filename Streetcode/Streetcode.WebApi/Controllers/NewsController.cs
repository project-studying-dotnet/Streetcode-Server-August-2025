using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.BLL.MediatR.Newss.GetById;
using Streetcode.BLL.MediatR.Newss.GetByUrl;
using Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl;
using Streetcode.BLL.MediatR.Newss.SortedByDateTime;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;

namespace Streetcode.WebApi.Controllers;

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
    [ProducesResponseType(typeof(NewsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNewsById([FromRoute] int id, CancellationToken ct)
    {
        var query = new GetNewsByIdQuery(id);
        var result = await Mediator.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{url}")]
    [ProducesResponseType(typeof(NewsDTOWithURLs), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpGet("{url}")]
    [ProducesResponseType(typeof(NewsDTOWithURLs), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNewsAndLinksByUrl([FromRoute] string url, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetNewsAndLinksByUrlQuery(url), ct);
        return HandleResult(result);
    }

    [HttpGet()]
    [ProducesResponseType(typeof(List<NewsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNewsSortedByDate(CancellationToken ct)
    {
        var query = new SortedByDateTimeQuery();
        var result = await Mediator.Send(query, ct);
        return HandleResult(result);
    }
}
