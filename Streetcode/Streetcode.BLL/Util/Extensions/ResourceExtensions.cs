namespace Streetcode.BLL.Util.Extensions;

public static class ResourceExtensions
{
    public static string FormatWith(this string resource, params object[] args)
    {
        return string.Format(resource, args);
    }
}
