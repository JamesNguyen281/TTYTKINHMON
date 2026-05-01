namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Middleware đếm lượt truy cập theo session.
/// Mỗi session mới (không có flag VisitedFlag) = 1 lượt; tăng Visited + Today + Online.
/// Khi session expire, ASP.NET không có hook chính thức nên giảm Online qua periodic check
/// hoặc dùng IHostedService — tạm thời chỉ tăng (như nhiều site CMS đời cũ).
/// </summary>
public class VisitorCounterMiddleware
{
    private readonly RequestDelegate _next;
    public VisitorCounterMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, VisitorCounter counter)
    {
        // Bỏ qua các request static + admin
        var path = context.Request.Path.Value ?? "";
        var isAsset = path.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase)
                   || path.StartsWith("/lib/",    StringComparison.OrdinalIgnoreCase)
                   || path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                   || path.EndsWith(".js",  StringComparison.OrdinalIgnoreCase)
                   || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                   || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   || path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase);

        if (!isAsset && context.Session.IsAvailable)
        {
            var seen = context.Session.GetString("VisitedFlag");
            if (string.IsNullOrEmpty(seen))
            {
                counter.OnSessionStart();
                context.Session.SetString("VisitedFlag", "1");
            }
        }

        await _next(context);
    }
}
