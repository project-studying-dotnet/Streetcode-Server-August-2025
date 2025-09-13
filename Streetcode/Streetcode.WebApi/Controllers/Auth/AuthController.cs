using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.MediatR.Users.Register;

namespace Streetcode.WebApi.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<RegisterUserResponseDTO>> Register([FromBody] RegisterUserDTO dto, CancellationToken ct)
        {
            var result = await Mediator.Send(new RegisterUserCommand(dto), ct);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }
    }
}
