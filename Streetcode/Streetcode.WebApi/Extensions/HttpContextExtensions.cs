using Microsoft.AspNetCore.Http;

namespace Streetcode.WebApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static void AppendJwtTokensToCookies(this HttpContext httpContext, string accessToken, string refreshToken)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token must be a non-empty string.", nameof(accessToken));
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token must be a non-empty string.", nameof(refreshToken));
            }

            if (httpContext.Response.HasStarted)
            {
                throw new InvalidOperationException("Response has already started; cannot append cookies.");
            }

            // Локальна функція-фабрика для створення CookieOptions
            CookieOptions CreateAuthCookieOptions(TimeSpan ttl) => new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                MaxAge = ttl
            };

            httpContext.Response.Cookies.Append("__Host-access_token", accessToken, CreateAuthCookieOptions(TimeSpan.FromHours(1)));

            httpContext.Response.Cookies.Append("__Host-refresh_token", refreshToken, CreateAuthCookieOptions(TimeSpan.FromDays(7)));
        }
    }
}
