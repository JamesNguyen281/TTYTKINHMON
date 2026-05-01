using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Data.Entities;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Base cho tất cả controller — load Site hiện tại vào ViewBag mỗi request,
/// expose CurrentUser + CurrentSiteId + Locate_Client (vi/en) từ session.
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly ISiteService SiteService;

    protected BaseController(ISiteService siteService)
    {
        SiteService = siteService;
    }

    /// <summary>User đang login (null nếu chưa).</summary>
    protected LoggedInUser? CurrentUser =>
        HttpContext.Session.GetObject<LoggedInUser>(Constants.UserSession);

    protected Guid CurrentSiteId
    {
        get
        {
            var s = HttpContext.Session.GetObject<Site>(Constants.SiteSession);
            return s?.Id ?? Guid.Empty;
        }
    }

    /// <summary>"vi" hoặc "en" — đọc từ session locate, mặc định "vi".</summary>
    protected string Locate_Client =>
        HttpContext.Session.GetString(Constants.LocateClient) ?? "vi";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Lazy-load Site vào session nếu chưa có
        var site = HttpContext.Session.GetObject<Site>(Constants.SiteSession);
        if (site == null)
        {
            site = await SiteService.GetCurrentAsync();
            if (site != null)
                HttpContext.Session.SetObject(Constants.SiteSession, site);
        }
        // Mặc định locate = "vi" nếu chưa có
        if (HttpContext.Session.GetString(Constants.LocateClient) == null)
            HttpContext.Session.SetString(Constants.LocateClient, "vi");

        ViewBag.Site = site;
        ViewBag.CurrentUser = CurrentUser;
        ViewBag.Locate = Locate_Client;
        await next();
    }
}
