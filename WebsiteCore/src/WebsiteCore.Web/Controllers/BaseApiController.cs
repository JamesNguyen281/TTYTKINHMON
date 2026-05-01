using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// API endpoints công khai phục vụ JS client.
/// Route: /base/{action}
/// </summary>
[Route("base/[action]")]
public class ClientApiController : BaseController
{
    public ClientApiController(ISiteService siteService) : base(siteService) { }

    /// <summary>Đổi ngôn ngữ — lưu vào session "locate_client".</summary>
    [HttpPost]
    public IActionResult ChangeCulture(string locate)
    {
        var loc = (locate == "en") ? "en" : "vi";
        HttpContext.Session.SetString(Constants.LocateClient, loc);
        return Json(new { locate = loc });
    }

    /// <summary>Đọc locate hiện tại.</summary>
    [HttpGet]
    public IActionResult GetSessionLocate()
    {
        return Json(new { locate = Locate_Client });
    }

    /// <summary>Đổi site (multi-site) — lưu Site mới vào session.</summary>
    [HttpPost]
    public async Task<IActionResult> ChangeSite(Guid siteID)
    {
        var site = await SiteService.GetByIdAsync(siteID);
        if (site == null) return Json(new { siteId = (Guid?)null });
        HttpContext.Session.SetObject(Constants.SiteSession, site);
        return Json(new { siteId = site.Id });
    }

    /// <summary>Trả Site hiện tại (cho client refresh thông tin liên hệ).</summary>
    [HttpPost]
    public async Task<IActionResult> GetSite(Guid siteId)
    {
        var s = await SiteService.GetByIdAsync(siteId) ?? await SiteService.GetCurrentAsync();
        if (s == null) return Json(new { currentSite = (object?)null });
        return Json(new
        {
            currentSite = new
            {
                id               = s.Id,
                name_company     = Locate_Client == "en" ? (s.NameCompanyE ?? s.NameCompanyL) : s.NameCompanyL,
                address          = Locate_Client == "en" ? (s.AddressE ?? s.AddressL) : s.AddressL,
                phone            = s.Phone,
                email            = s.Email,
                fax              = s.Fax,
                emergency_number = s.EmergencyNumber,
                hotline          = s.Hotline,
                map              = s.Map ?? ""
            }
        });
    }

    /// <summary>Trả id site đầu tiên active.</summary>
    [HttpGet]
    public async Task<IActionResult> GetFirstSiteId()
    {
        var s = await SiteService.GetCurrentAsync();
        return Json(new { firstSite = s?.Id });
    }
}
