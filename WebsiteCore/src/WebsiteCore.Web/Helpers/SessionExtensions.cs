using System.Text.Json;

namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Session trong ASP.NET Core chỉ lưu byte/string. Dùng JSON để serialize object.
/// Pattern: <c>HttpContext.Session.SetObject("USER", loggedInUser)</c>.
/// </summary>
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetObject<T>(this ISession session, string key)
    {
        var s = session.GetString(key);
        return s == null ? default : JsonSerializer.Deserialize<T>(s);
    }
}
