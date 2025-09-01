using System.Security.Claims;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.Interfaces.Jwt;

public interface IJwtService
{
    Task<string?> GenerateTokenAsync(int userId, CancellationToken ct = default);
    ClaimsPrincipal? ValidateToken(string token);
    int? GetUserIdFromToken(string token);
}