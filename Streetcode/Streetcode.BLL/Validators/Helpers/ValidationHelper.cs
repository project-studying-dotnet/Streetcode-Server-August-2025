namespace Streetcode.BLL.Validators.Helpers;

public static class ValidationHelper
{
    public static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}