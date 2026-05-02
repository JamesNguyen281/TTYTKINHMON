using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data.Entities;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class DoctorSchedulesController : BaseController
{
    private readonly IDoctorScheduleService _service;
    private readonly IDoctorService _doctorService;
    private readonly IDepartmentService _deptService;
    private readonly IAuditService _audit;

    public DoctorSchedulesController(
        ISiteService siteService,
        IDoctorScheduleService service,
        IDoctorService doctorService,
        IDepartmentService deptService,
        IAuditService audit) : base(siteService)
    {
        _service = service;
        _doctorService = doctorService;
        _deptService = deptService;
        _audit = audit;
    }

    public async Task<IActionResult> Index(Guid? doctorId, Guid? departmentId, byte? weekday, string? session)
    {
        ViewBag.Title = "Lịch trực bác sĩ";
        var list = await _service.GetAllActiveAsync();
        // Filter
        if (doctorId.HasValue && doctorId.Value != Guid.Empty)
            list = list.Where(s => s.DoctorId == doctorId.Value).ToList();
        if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            list = list.Where(s => s.DepartmentId == departmentId.Value).ToList();
        if (weekday.HasValue)
            list = list.Where(s => s.Weekday == weekday.Value).ToList();
        if (!string.IsNullOrWhiteSpace(session))
            list = list.Where(s => s.Session == session).ToList();

        ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        ViewBag.SelectedDoctorId = doctorId;
        ViewBag.SelectedDeptId = departmentId;
        ViewBag.SelectedWeekday = weekday;
        ViewBag.SelectedSession = session;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Thêm lịch trực";
        ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View("Form", new DoctorSchedule
        {
            ActiveFlag = 1,
            ValidFrom = DateOnly.FromDateTime(DateTime.Today),
            Session = Constants.SessionMorning,
            MaxPatients = Constants.DefaultQuotaPerSession
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorSchedule s)
    {
        ValidateSchedule(s);
        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View("Form", s);
        }
        s.CreatedBy = CurrentUser?.Id;
        await _service.CreateAsync(s);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo lịch trực", $"{u.UserName} tạo lịch BS={s.DoctorId} weekday={s.Weekday} {s.Session} {s.ValidFrom:yyyy-MM-dd}→{(s.ValidTo?.ToString("yyyy-MM-dd") ?? "∞")}");
        TempData["Success"] = "Đã thêm lịch.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateSchedule(DoctorSchedule s)
    {
        if (s.Weekday < 1 || s.Weekday > 7)
            ModelState.AddModelError(nameof(s.Weekday), "Weekday phải trong khoảng 1 (CN) → 7 (T7).");
        if (s.Session != Constants.SessionMorning && s.Session != Constants.SessionAfternoon)
            ModelState.AddModelError(nameof(s.Session), "Buổi phải là 'morning' hoặc 'afternoon'.");
        if (s.ValidTo.HasValue && s.ValidTo.Value < s.ValidFrom)
            ModelState.AddModelError(nameof(s.ValidTo), "Ngày kết thúc không được trước ngày bắt đầu.");
        if (s.MaxPatients.HasValue && (s.MaxPatients < 1 || s.MaxPatients > 500))
            ModelState.AddModelError(nameof(s.MaxPatients), "Quota phải trong khoảng 1–500.");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var s = await _service.GetByIdAsync(id);
        if (s == null) return NotFound();
        ViewBag.Title = "Sửa lịch trực";
        ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View("Form", s);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DoctorSchedule s)
    {
        ValidateSchedule(s);
        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View("Form", s);
        }
        await _service.UpdateAsync(s);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa lịch trực",
            $"{u.UserName} sửa lịch ID={s.Id} weekday={s.Weekday} {s.Session} room={s.Room} max={s.MaxPatients} cycle={s.ValidFrom:yyyy-MM-dd}→{(s.ValidTo?.ToString("yyyy-MM-dd") ?? "∞")}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá lịch trực", $"{u.UserName} xoá lịch ID={id}");
        TempData["Success"] = "Đã xoá (ẩn).";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Form chọn tháng để auto-gen. Mặc định = tháng tiếp theo (vì admin thường thao tác cuối tháng).
    /// </summary>
    [HttpGet]
    public IActionResult AutoGenerate()
    {
        ViewBag.Title = "Tự sinh lịch trực tháng";
        var nextMonth = DateTime.Today.AddMonths(1);
        ViewBag.DefaultYear  = nextMonth.Year;
        ViewBag.DefaultMonth = nextMonth.Month;
        return View();
    }

    /// <summary>
    /// Tự sinh lịch trực cho 1 tháng. Idempotent — chạy 2 lần không trùng.
    /// Chỉ ADMIN (qua [StaffAuthorize] ở class) + CSRF + site scoping.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoGenerate(int year, int month, bool confirm)
    {
        if (!confirm)
        {
            TempData["Error"] = "Vui lòng tick xác nhận trước khi chạy.";
            return RedirectToAction(nameof(AutoGenerate));
        }
        if (year < 2020 || year > 2100 || month < 1 || month > 12)
        {
            TempData["Error"] = "Tháng/năm không hợp lệ.";
            return RedirectToAction(nameof(AutoGenerate));
        }

        var u = CurrentUser!;
        var rs = await _service.GenerateMonthlyScheduleAsync(year, month, CurrentSiteId, u.Id);
        await _audit.LogAsync(u.Id, "Auto-gen lịch trực tháng",
            $"{u.UserName} auto-gen {month:00}/{year} site={CurrentSiteId} processed={rs.DoctorsProcessed} created={rs.Created} skipped={rs.SkippedExisting}");

        if (rs.Created == 0 && rs.SkippedExisting > 0)
            TempData["Success"] = $"Tất cả {rs.SkippedExisting} BS đã có lịch tháng {month:00}/{year} — không tạo trùng.";
        else if (rs.DoctorsProcessed == 0)
            TempData["Error"] = "Không có BS nào active với khoa active để tạo lịch.";
        else
            TempData["Success"] = $"Đã tạo {rs.Created} lịch cho {rs.DoctorsProcessed - rs.SkippedExisting} BS (skip {rs.SkippedExisting} BS đã có lịch).";

        return RedirectToAction(nameof(Index));
    }
}
