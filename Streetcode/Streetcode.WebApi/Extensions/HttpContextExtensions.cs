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

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Запобігає доступу до куків через клієнтський JavaScript
                Secure = true,   // Куки будуть надсилатися лише через HTTPS
                SameSite = SameSiteMode.Strict, // Запобігає CSRF-атакам
                Expires = DateTime.UtcNow.AddHours(1) // Час життя токена
            };

            httpContext.Response.Cookies.Append("access_token", accessToken, cookieOptions);

            // Додаємо Refresh Token до куків
            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            httpContext.Response.Cookies.Append("refresh_token", refreshToken, refreshTokenCookieOptions);
        }
    }
}
