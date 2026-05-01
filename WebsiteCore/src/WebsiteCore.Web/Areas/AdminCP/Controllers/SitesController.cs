using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data.Entities;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class SitesController : BaseController
{
    private readonly IAuditService _audit;
    private readonly IWebHostEnvironment _env;

    public SitesController(ISiteService siteService, IAuditService audit, IWebHostEnvironment env) : base(siteService)
    {
        _audit = audit;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Cấu hình site";
        var list = await SiteService.GetAllAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var s = await SiteService.GetByIdAsync(id);
        if (s == null) return NotFound();
        ViewBag.Title = "Sửa site: " + s.NameCompanyL;
        return View(s);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Site site, IFormFile? logoFile, IFormFile? faviconFile)
    {
        if (!ModelState.IsValid) return View(site);

        // Upload logo / favicon nếu admin chọn file mới — replace LogoUrl / Favicon trong DB
        var savedLogo = await FileUploadHelper.SaveImageAsync(logoFile, _env, "sites");
        if (!string.IsNullOrEmpty(savedLogo)) site.LogoUrl = "/" + savedLogo;
        var savedFav = await FileUploadHelper.SaveImageAsync(faviconFile, _env, "sites");
        if (!string.IsNullOrEmpty(savedFav)) site.Favicon = "/" + savedFav;

        await SiteService.UpdateAsync(site);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa site", $"{u.UserName} sửa site: {site.NameCompanyL}");
        // Refresh session site — phải drop cache để mọi portal load Site mới
        HttpContext.Session.Remove(Constants.SiteSession);
        TempData["Success"] = "Đã cập nhật site.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var s = await SiteService.GetByIdAsync(id);
        if (s == null) return NotFound();

        var (ok, err) = await SiteService.DeleteAsync(id);
        var u = CurrentUser!;
        if (ok)
        {
            await _audit.LogAsync(u.Id, "Xoá site", $"{u.UserName} xoá site: {s.NameCompanyL} (id={id})");
            HttpContext.Session.Remove(Constants.SiteSession);
            TempData["Success"] = $"Đã xoá site \"{s.NameCompanyL}\".";
        }
        else
        {
            TempData["Error"] = err ?? "Không xoá được site.";
        }
        return RedirectToAction(nameof(Index));
    }
}
