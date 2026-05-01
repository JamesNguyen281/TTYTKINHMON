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
public class DoctorsController : BaseController
{
    private readonly IDoctorService _service;
    private readonly IDepartmentService _deptService;
    private readonly IAuditService _audit;
    private readonly IWebHostEnvironment _env;
    private readonly TtytlpDbContext _db;

    public DoctorsController(
        ISiteService siteService,
        IDoctorService service,
        IDepartmentService deptService,
        IAuditService audit,
        IWebHostEnvironment env,
        TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _deptService = deptService;
        _audit = audit;
        _env = env;
        _db = db;
    }

    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20)
    {
        ViewBag.Title = "Quản lý bác sĩ";
        ViewBag.Q = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = await _service.CountAsync(CurrentSiteId, q);
        var list = await _service.SearchAsync(CurrentSiteId, q, page, pageSize);
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Thêm bác sĩ";
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View("Form", new Doctor { ActiveFlag = 1, ShowOnHome = true, IsPartner = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Doctor d, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View("Form", d);
        }
        var saved = await FileUploadHelper.SaveImageAsync(imageFile, _env, "doctors");
        if (!string.IsNullOrEmpty(saved)) d.ImagePath = saved;
        await _service.CreateAsync(d);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo bác sĩ", $"{u.UserName} tạo BS: {d.NameL}");
        TempData["Success"] = "Đã thêm bác sĩ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound();
        ViewBag.Title = "Sửa bác sĩ: " + d.NameL;
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View("Form", d);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Doctor d, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View("Form", d);
        }
        var saved = await FileUploadHelper.SaveImageAsync(imageFile, _env, "doctors");
        if (!string.IsNullOrEmpty(saved)) d.ImagePath = saved;
        await _service.UpdateAsync(d);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa bác sĩ", $"{u.UserName} sửa BS: {d.NameL}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound();
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá bác sĩ", $"{u.UserName} xoá BS: {d.NameL}");
        TempData["Success"] = "Đã xoá (ẩn) bác sĩ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var d = await _db.Doctors.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        d.ActiveFlag = d.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle bác sĩ",
            $"{u.UserName} {(d.ActiveFlag == 1 ? "HIỆN" : "ẨN")} bs '{d.NameL}'");
        TempData["Success"] = d.ActiveFlag == 1 ? "Đã hiển thị bác sĩ." : "Đã ẩn bác sĩ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleShowHome(Guid id)
    {
        var d = await _db.Doctors.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        d.ShowOnHome = !(d.ShowOnHome ?? false);
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle hiển thị bs",
            $"{u.UserName} {(d.ShowOnHome == true ? "BẬT" : "TẮT")} ShowOnHome bs '{d.NameL}'");
        TempData["Success"] = d.ShowOnHome == true ? "Đã bật hiển thị trang chủ." : "Đã ẩn khỏi trang chủ.";
        return RedirectToAction(nameof(Index));
    }
}
