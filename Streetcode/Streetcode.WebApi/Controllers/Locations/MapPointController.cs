using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.Locations.GetAll;
using Streetcode.BLL.MediatR.Locations.Update;
using Streetcode.BLL.MediatR.Locations.Delete;

namespace Streetcode.WebApi.Controllers.Locations;

public class MapPointController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllMapPointsQuery()));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id)
    {
        return HandleResult(await Mediator.Send(new UpdateMapPointCommand(id)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return HandleResult(await Mediator.Send(new DeleteMapPointCommand(id)));
    }
}
