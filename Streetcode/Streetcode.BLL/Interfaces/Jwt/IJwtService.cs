using System.Security.Claims;
using FluentResults;
using Streetcode.BLL.DTO.Users;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.Interfaces.Jwt;

public interface IJwtTokenService
{
    Task<Result<LoginResultDTO>> GenerateTokenAsync(int userId);
    Result<ClaimsPrincipal> ValidateToken(string token);
    Result<int> GetUserIdFromToken(string token);
    Task<Result<LoginResultDTO>> RefreshTokenAsync(string token, string refreshToken);
}