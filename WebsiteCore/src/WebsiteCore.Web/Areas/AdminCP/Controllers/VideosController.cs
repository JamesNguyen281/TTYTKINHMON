using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class VideosController : BaseController
{
    private readonly IVideoService _service;
    private readonly IAuditService _audit;
    private readonly TtytlpDbContext _db;

    public VideosController(ISiteService siteService, IVideoService service, IAuditService audit, TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _audit = audit;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Quản lý video";
        var list = await _service.GetAllAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Thêm video";
        return View("Form", new Video { Status = 1, SiteId = CurrentSiteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Video v)
    {
        if (!ModelState.IsValid) return View("Form", v);
        v.SiteId = CurrentSiteId;
        v.CreatedByUser = CurrentUser?.Id;
        await _service.CreateAsync(v);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo video", $"{u.UserName} tạo: {v.VideoTitleL}");
        TempData["Success"] = "Đã thêm video.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var v = await _service.GetByIdAsync(id);
        if (v == null) return NotFound();
        ViewBag.Title = "Sửa video";
        return View("Form", v);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Video v)
    {
        if (!ModelState.IsValid) return View("Form", v);
        await _service.UpdateAsync(v);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa video", $"{u.UserName} sửa: {v.VideoTitleL}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá video", $"{u.UserName} xoá video ID={id}");
        TempData["Success"] = "Đã xoá (ẩn).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var v = await _db.Videos.FirstOrDefaultAsync(x => x.VideoId == id);
        if (v == null) return NotFound();
        v.Status = v.Status == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle video",
            $"{u.UserName} {(v.Status == 1 ? "BẬT" : "TẮT")} video '{v.VideoTitleL}'");
        TempData["Success"] = v.Status == 1 ? "Đã bật video." : "Đã tắt video.";
        return RedirectToAction(nameof(Index));
    }
}
