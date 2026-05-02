namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Middleware đếm lượt truy cập theo session.
/// Mỗi request page (không phải static asset) sẽ Touch session vào sliding window 15'.
/// Session mới (chưa có VisitedFlag) thêm 1 vào Today + Total file-backed.
/// </summary>
public class VisitorCounterMiddleware
{
    private readonly RequestDelegate _next;
    public VisitorCounterMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, VisitorCounter counter)
    {
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
            counter.Touch(context.Session.Id);

            var seen = context.Session.GetString("VisitedFlag");
            if (string.IsNullOrEmpty(seen))
            {
                counter.OnNewSession();
                context.Session.SetString("VisitedFlag", "1");
            }
        }

        await _next(context);
    }
}
