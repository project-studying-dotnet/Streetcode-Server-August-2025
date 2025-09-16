using Streetcode.DAL.Enums;
using System.Security.Claims;

namespace Streetcode.WebApi.Utils;

public static class AuthHelper
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim))
            throw new UnauthorizedAccessException("UserId not found in token.");

        return int.Parse(claim);
    }

    public static UserRole GetUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
            throw new UnauthorizedAccessException("Role not found in token.");

        if (!Enum.TryParse<UserRole>(roleClaim, out var role))
            throw new UnauthorizedAccessException($"Invalid role in token: {roleClaim}");

        return role;
    }

    public static IEnumerable<Claim> GetAllClaims(ClaimsPrincipal user) => user.Claims;
}