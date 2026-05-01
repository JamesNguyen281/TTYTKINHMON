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
public class DepartmentsController : BaseController
{
    private readonly IDepartmentService _service;
    private readonly IAuditService _auditService;
    private readonly TtytlpDbContext _db;

    public DepartmentsController(
        ISiteService siteService,
        IDepartmentService service,
        IAuditService auditService,
        TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _auditService = auditService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Quản lý chuyên khoa";
        var list = await _service.GetAllBySiteAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Tạo chuyên khoa mới";
        return View(new Department { ActiveFlag = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department dept)
    {
        if (!ModelState.IsValid) return View(dept);
        var u = CurrentUser!;
        dept.CreatedByUserId = u.Id;
        await _service.CreateAsync(dept, CurrentSiteId);
        await _auditService.LogAsync(u.Id, "Tạo chuyên khoa", $"{u.UserName} tạo khoa: {dept.NameL}");
        TempData["Success"] = "Đã tạo chuyên khoa.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound();
        ViewBag.Title = "Sửa chuyên khoa: " + d.NameL;
        return View(d);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Department dept)
    {
        if (!ModelState.IsValid) return View(dept);
        var u = CurrentUser!;
        await _service.UpdateAsync(dept);
        await _auditService.LogAsync(u.Id, "Sửa chuyên khoa", $"{u.UserName} sửa khoa: {dept.NameL}");
        TempData["Success"] = "Đã cập nhật chuyên khoa.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound();
        var u = CurrentUser!;
        await _service.DeleteAsync(id);
        await _auditService.LogAsync(u.Id, "Xoá chuyên khoa", $"{u.UserName} xoá khoa: {d.NameL}");
        TempData["Success"] = "Đã xoá (ẩn) chuyên khoa.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var d = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        var oldVal = d.ActiveFlag;
        d.ActiveFlag = d.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        // L2: audit toggle
        var u = CurrentUser!;
        await _auditService.LogAsync(u.Id, "Toggle khoa",
            $"{u.UserName} {(d.ActiveFlag == 1 ? "BẬT" : "TẮT")} khoa '{d.NameL}' ({oldVal}→{d.ActiveFlag})");
        TempData["Success"] = d.ActiveFlag == 1 ? "Đã bật chuyên khoa." : "Đã tắt chuyên khoa.";
        return RedirectToAction(nameof(Index));
    }
}
