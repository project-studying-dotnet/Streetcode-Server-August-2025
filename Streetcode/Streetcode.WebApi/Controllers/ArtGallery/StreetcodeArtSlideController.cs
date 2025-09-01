using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.ArtGallery;

namespace Streetcode.WebApi.Controllers.ArtGallery;

public class StreetcodeArtSlideController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] uint streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetArtSlidesByStreetcodeIdQuery(streetcodeId)));
    }
}
