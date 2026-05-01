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
public class SlidesController : BaseController
{
    private readonly ISlideService _service;
    private readonly IAuditService _audit;
    private readonly IWebHostEnvironment _env;
    private readonly TtytlpDbContext _db;

    public SlidesController(ISiteService siteService, ISlideService service, IAuditService audit, IWebHostEnvironment env, TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _audit = audit;
        _env = env;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Quản lý slide / banner";
        var list = await _service.GetAllAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Thêm slide";
        return View("Form", new Slide { ActiveFlag = 1, SiteId = CurrentSiteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Slide s, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View("Form", s);
        s.SiteId = CurrentSiteId;
        var saved = await FileUploadHelper.SaveImageAsync(imageFile, _env, "slides");
        if (!string.IsNullOrEmpty(saved)) s.ImagePath = saved;
        await _service.CreateAsync(s);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo slide", $"{u.UserName} tạo slide: {s.TitleL}");
        TempData["Success"] = "Đã thêm slide.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var s = await _service.GetByIdAsync(id);
        if (s == null) return NotFound();
        ViewBag.Title = "Sửa slide";
        return View("Form", s);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Slide s, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View("Form", s);
        var saved = await FileUploadHelper.SaveImageAsync(imageFile, _env, "slides");
        if (!string.IsNullOrEmpty(saved)) s.ImagePath = saved;
        await _service.UpdateAsync(s);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa slide", $"{u.UserName} sửa slide: {s.TitleL}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá slide", $"{u.UserName} xoá slide ID={id}");
        TempData["Success"] = "Đã xoá (ẩn).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var s = await _db.Slides.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        s.ActiveFlag = s.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle slide",
            $"{u.UserName} {(s.ActiveFlag == 1 ? "BẬT" : "TẮT")} slide '{s.TitleL}'");
        TempData["Success"] = s.ActiveFlag == 1 ? "Đã bật slide." : "Đã tắt slide.";
        return RedirectToAction(nameof(Index));
    }
}
