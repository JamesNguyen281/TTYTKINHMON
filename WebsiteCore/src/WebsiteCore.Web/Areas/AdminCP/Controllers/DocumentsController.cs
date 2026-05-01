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
public class DocumentsController : BaseController
{
    private readonly IDocumentService _service;
    private readonly IAuditService _audit;
    private readonly TtytlpDbContext _db;

    public DocumentsController(ISiteService siteService, IDocumentService service, IAuditService audit, TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _audit = audit;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Quản lý tài liệu";
        var list = await _service.GetAllAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Thêm tài liệu";
        return View("Form", new Document { ActiveFlag = 1, SiteId = CurrentSiteId, DocumentDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Document d)
    {
        if (!ModelState.IsValid) return View("Form", d);
        d.SiteId = CurrentSiteId;
        d.CreatedByUserId = CurrentUser?.Id;
        await _service.CreateAsync(d);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo tài liệu", $"{u.UserName} tạo: {d.DocumentName}");
        TempData["Success"] = "Đã thêm tài liệu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound();
        ViewBag.Title = "Sửa tài liệu";
        return View("Form", d);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Document d)
    {
        if (!ModelState.IsValid) return View("Form", d);
        d.LuUserId = CurrentUser?.Id;
        await _service.UpdateAsync(d);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa tài liệu", $"{u.UserName} sửa: {d.DocumentName}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá tài liệu", $"{u.UserName} xoá ID={id}");
        TempData["Success"] = "Đã xoá (ẩn).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var d = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        d.ActiveFlag = d.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle tài liệu",
            $"{u.UserName} {(d.ActiveFlag == 1 ? "BẬT" : "TẮT")} tài liệu '{d.DocumentName}'");
        TempData["Success"] = d.ActiveFlag == 1 ? "Đã bật tài liệu." : "Đã tắt tài liệu.";
        return RedirectToAction(nameof(Index));
    }
}
