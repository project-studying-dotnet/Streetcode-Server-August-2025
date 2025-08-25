using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.BLL.MediatR.Newss.GetById;
using Streetcode.BLL.MediatR.Newss.GetByUrl;
using Streetcode.BLL.MediatR.Newss.Update;

namespace Streetcode.WebApi.Controllers;

/// <summary>
/// Not finished controller created for testing purposes
/// in the future it will be worth refining and rechecking
/// </summary>
public class NewsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateNews([FromBody] NewsDTO newsDto)
    {
        var command = new CreateNewsCommand(newsDto);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNews()
    {
        var query = new GetAllNewsQuery();
        var result = await Mediator.Send(query);
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

    [HttpPut]
    public async Task<IActionResult> UpdateNews([FromBody] NewsDTO newsDto)
    {
        var command = new UpdateNewsCommand(newsDto);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNews([FromRoute] int id)
    {
        var command = new DeleteNewsCommand(id);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
