using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.DTO.Users.GoogleLogin;
using Streetcode.BLL.Interfaces.Google;

namespace Streetcode.BLL.Services.Google;

public class GoogleService : IGoogleService
{
    private const string GoogleScheme = "Google";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public GoogleService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GoogleUserInfoDTO> GetGoogleUserInfoAsync()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        var authResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(GoogleScheme);
        if (!authResult.Succeeded)
        {
            throw new InvalidOperationException($"Google authentication failed");
        }

        var claims = authResult.Principal.Claims;
        var userInfo = new GoogleUserInfoDTO
        {
            Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!,
            GivenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value!,
            FamilyName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value!,
            Subject = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!
        };

        if (string.IsNullOrEmpty(userInfo.Email) || string.IsNullOrEmpty(userInfo.Subject))
        {
            throw new InvalidOperationException("Google user info is missing");
        }

        return userInfo;
    }
}
