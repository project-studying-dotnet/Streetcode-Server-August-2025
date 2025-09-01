using System.Security.Claims;
using Streetcode.BLL.DTO.Users;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.Interfaces.Jwt;

public interface IJwtService
{
    Task<LoginResultDTO> GenerateTokenAsync(int userId);
    ClaimsPrincipal? ValidateToken(string token);
    int? GetUserIdFromToken(string token);
    Task<LoginResultDTO> RefreshTokenAsync(string token, string refreshToken);
}