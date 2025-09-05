using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.Services.JwtService;

namespace Streetcode.WebApi.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtEnvironmentVariables _jwtSettings;
        private readonly IJwtTokenService _jwtService;

        public UserController(IOptions<JwtEnvironmentVariables> jwtSettings, IJwtTokenService jwtService)
        {
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
        }

        /* Endpoints for testing
        // Endpoint for testing purposes, can be removed later
        [HttpGet("jwt-settings")]
        public IActionResult GetJwtSettings()
        {
            return Ok(new
            {
                _jwtSettings.SecretKey,
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                _jwtSettings.ExpiryMinutes
            });
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("generate-token")]
        public async Task<IActionResult> GenerateToken(int userId)
        {
            var result = await _jwtService.GenerateTokenAsync(userId);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var result = _jwtService.ValidateToken(token);

            if (result.IsFailed)
            {
                return Unauthorized(result.Errors.Select(e => e.Message));
            }

            var claims = result.Value.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("get-user-id")]
        public IActionResult GetUserIdFromToken([FromBody] string token)
        {
            var result = _jwtService.GetUserIdFromToken(token);

            if (result.IsFailed)
            {
                return Unauthorized(result.Errors.Select(e => e.Message));
            }

            return Ok(new { userId = result.Value });
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResultDTO>> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _jwtService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

            if (result.IsFailed)
            {
                return Unauthorized(result.Errors.Select(e => e.Message));
            }

            return Ok(result.Value);
        }*/
    }
}