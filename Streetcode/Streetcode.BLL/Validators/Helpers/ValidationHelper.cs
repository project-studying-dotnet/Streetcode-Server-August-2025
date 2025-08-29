using System.Text.RegularExpressions;

namespace Streetcode.BLL.Validators.Helpers;

public static class ValidationHelper
{
    public static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }

        return true;
    }
}