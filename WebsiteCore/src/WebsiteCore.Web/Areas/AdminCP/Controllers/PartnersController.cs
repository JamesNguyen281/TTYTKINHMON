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
public class PartnersController : BaseController
{
    private readonly IPartnerService _service;
    private readonly IAuditService _audit;
    private readonly TtytlpDbContext _db;

    public PartnersController(ISiteService siteService, IPartnerService service, IAuditService audit, TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _audit = audit;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Quản lý đối tác";
        var list = await _service.GetAllAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Thêm đối tác";
        return View("Form", new Partner { ActiveFlag = 1, SiteId = CurrentSiteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Partner p)
    {
        if (!ModelState.IsValid) return View("Form", p);
        p.SiteId = CurrentSiteId;
        await _service.CreateAsync(p);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo đối tác", $"{u.UserName} tạo: {p.NameL}");
        TempData["Success"] = "Đã thêm đối tác.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var p = await _service.GetByIdAsync(id);
        if (p == null) return NotFound();
        ViewBag.Title = "Sửa đối tác";
        return View("Form", p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Partner p)
    {
        if (!ModelState.IsValid) return View("Form", p);
        await _service.UpdateAsync(p);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa đối tác", $"{u.UserName} sửa: {p.NameL}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá đối tác", $"{u.UserName} xoá ID={id}");
        TempData["Success"] = "Đã xoá (ẩn).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var p = await _db.Partners.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        p.ActiveFlag = p.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle đối tác",
            $"{u.UserName} {(p.ActiveFlag == 1 ? "BẬT" : "TẮT")} '{p.NameL}'");
        TempData["Success"] = p.ActiveFlag == 1 ? "Đã bật đối tác." : "Đã tắt đối tác.";
        return RedirectToAction(nameof(Index));
    }
}
