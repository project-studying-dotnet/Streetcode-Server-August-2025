using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.FavouriteStreetcode.Delete;
using Streetcode.BLL.MediatR.FavouriteStreetcode.GetFavoritesByUserId;
using Streetcode.WebApi.Utils;

namespace Streetcode.WebApi.Controllers.FavouriteStreetcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouriteStreetcodeController : BaseApiController
    {
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteFavouriteStreetcode([FromRoute] int id, CancellationToken ct)
        {
            var userId = AuthHelper.GetUserId(HttpContext.User);

            var command = new DeleteFavouriteStreetcodeCommand(id, userId);
            var result = await Mediator.Send(command, ct);

            return HandleResult(result);
        }

        // [Authorize]
        [HttpGet("{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByUserId([FromRoute] int userId)
        {
            var result = await Mediator.Send(new GetFavoritesByUserIdQuery(userId));

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }
    }
}
