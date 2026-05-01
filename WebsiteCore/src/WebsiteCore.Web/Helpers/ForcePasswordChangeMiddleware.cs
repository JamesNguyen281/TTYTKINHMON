using WebsiteCore.Business;

namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Middleware: nếu session có cờ FORCE_PWD_CHANGE → chuyển hướng mọi request về /doi-mat-khau
/// (trừ chính trang đó, đăng xuất, static asset).
/// </summary>
public class ForcePasswordChangeMiddleware
{
    private readonly RequestDelegate _next;
    public ForcePasswordChangeMiddleware(RequestDelegate next) => _next = next;

    private static readonly string[] Allowed = new[]
    {
        "/doi-mat-khau",
        "/dang-xuat",
        "/AdminCP/Login/Logout",
        "/base/",                  // ngôn ngữ, site config
        "/assets/",
        "/favicon",
        "/css/", "/js/", "/lib/"
    };

    public async Task Invoke(HttpContext ctx)
    {
        // Session sẽ được setup sau UseSession middleware
        var force = ctx.Session?.GetString(Constants.ForcePwdChange);
        if (force == "1")
        {
            var path = ctx.Request.Path.Value ?? "";
            bool allow = Allowed.Any(a => path.StartsWith(a, StringComparison.OrdinalIgnoreCase));
            if (!allow)
            {
                ctx.Response.Redirect("/doi-mat-khau");
                return;
            }
        }
        await _next(ctx);
    }
}
