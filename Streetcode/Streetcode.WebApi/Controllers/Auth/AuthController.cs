using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.DTO.Users.Logout;
using Streetcode.BLL.MediatR.Auth.GoogleLogin;
using Streetcode.BLL.MediatR.Users.Logout;
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

        [HttpPost("logout")]
        public async Task<ActionResult<LogoutResponseDTO>> Logout([FromBody] LogoutRequestDTO dto, CancellationToken ct)
        {
            var result = await Mediator.Send(new LogoutUserCommand(dto), ct);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }

        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            var props = new AuthenticationProperties { RedirectUri = "/api/auth/google-callback" };

            return Challenge(props, "Google");
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(CancellationToken ct)
        {
            var result = await Mediator.Send(new GoogleLoginQuery(), ct);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }
    }
}
