using System.Text.RegularExpressions;

namespace Streetcode.BLL.Validators.Helpers;

public static class ValidationHelper
{
    public static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        if (string.IsNullOrWhiteSpace(uri.Host))
            return false;

        return true;
    }
    public static bool BeHttpOrHttpsUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    public static bool IsSlug(string value)
        => !string.IsNullOrWhiteSpace(value)
           && Regex.IsMatch(value, "^[a-z0-9]+(?:-[a-z0-9]+)*$");

    public static bool BeAbsoluteUrlOrSlug(string value)
        => BeHttpOrHttpsUrl(value) || IsSlug(value);
}