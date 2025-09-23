using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.DTO.Users.ChangePassword;
using Streetcode.BLL.DTO.Users.Logout;
using Streetcode.BLL.MediatR.Users.ChangePassword;
using Streetcode.BLL.MediatR.Users.Logout;
using Streetcode.BLL.MediatR.Users.Register;

namespace Streetcode.WebApi.Controllers.Auth
{
    public class AuthController : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<RegisterUserResponseDTO>> Register([FromBody] RegisterUserDTO dto, CancellationToken ct)
        {
            var result = await Mediator.Send(new RegisterUserCommand(dto), ct);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<LogoutResponseDTO>> Logout([FromBody] LogoutRequestDTO dto, CancellationToken ct)
        {
            var result = await Mediator.Send(new LogoutUserCommand(dto), ct);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var result = await Mediator.Send(new ChangePasswordCommand(dto));

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
