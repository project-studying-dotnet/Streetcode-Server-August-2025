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
        private readonly IJwtService _jwtService;

        public UserController(IOptions<JwtEnvironmentVariables> jwtSettings, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
        }

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

            return Ok(result);
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid or expired token."
                });
            }

            var claims = principal.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("get-user-id")]
        public IActionResult GetUserIdFromToken([FromBody] string token)
        {
            var userId = _jwtService.GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid token."
                });
            }

            return Ok(new { userId });
        }

        // Endpoint for testing purposes, can be removed later
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResultDTO>> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var result = await _jwtService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}