using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Cổng lễ tân — chỉ RECEPTION + ADMIN truy cập được.
/// Workflow: Bệnh nhân đặt lịch (pending) → Lễ tân duyệt (confirmed + sinh booking_code) → BN đến → Lễ tân check-in.
/// </summary>
[StaffAuthorize(Constants.ReceptionGroup)]
public class LeTanController : BaseController
{
    private readonly IAppointmentService _apptService;
    private readonly IAuditService _auditService;
    private readonly IDoctorScheduleService _scheduleService;
    private readonly IDoctorService _doctorService;
    private readonly IDepartmentService _deptService;
    private readonly IQuotaService _quotaService;

    public LeTanController(
        ISiteService siteService,
        IAppointmentService apptService,
        IAuditService auditService,
        IDoctorScheduleService scheduleService,
        IDoctorService doctorService,
        IDepartmentService deptService,
        IQuotaService quotaService) : base(siteService)
    {
        _apptService = apptService;
        _auditService = auditService;
        _scheduleService = scheduleService;
        _doctorService = doctorService;
        _deptService = deptService;
        _quotaService = quotaService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Trang lễ tân";
        var pending = await _apptService.GetByStatusAsync(Constants.ApptPending, CurrentSiteId);
        var today   = await _apptService.GetTodayConfirmedAsync(CurrentSiteId);
        ViewBag.PendingCount    = pending.Count;
        ViewBag.TodayCount      = today.Count;
        ViewBag.CheckedInCount  = today.Count(t => t.CheckedIn);
        ViewBag.UnassignedCount = today.Count(t => !t.DoctorId.HasValue);
        return View();
    }

    /// <summary>JSON counts dùng cho polling real-time tổng quan.</summary>
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Counts()
    {
        var pending = (await _apptService.GetByStatusAsync(Constants.ApptPending, CurrentSiteId)).Count;
        var today   = await _apptService.GetTodayConfirmedAsync(CurrentSiteId);
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return Json(new
        {
            pending,
            today      = today.Count,
            checkedIn  = today.Count(t => t.CheckedIn),
            unassigned = today.Count(t => !t.DoctorId.HasValue)
        });
    }

    public async Task<IActionResult> Appointments(string status = "pending")
    {
        ViewBag.Title = "Hàng đợi lịch hẹn";
        ViewBag.Status = status;
        var list = await _apptService.GetByStatusAsync(status, CurrentSiteId);
        return View(list);
    }

    /// <summary>
    /// Tra cứu lịch theo SĐT — giúp khách vãng lai (không có account) khi gọi đến hỏi lại.
    /// Site scoping qua CurrentSiteId chống lễ tân site A xem được lịch site B.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TimTheoSdt(string? phone)
    {
        ViewBag.Title = "Tra cứu lịch theo số điện thoại";
        ViewBag.Phone = phone;
        ViewBag.Results = !string.IsNullOrWhiteSpace(phone)
            ? await _apptService.GetByPhoneAsync(phone.Trim(), CurrentSiteId)
            : null;
        return View();
    }

    /// <summary>
    /// Lịch theo ngày — hiển thị tất cả lịch của 1 ngày (mọi status), kèm indicator check-in.
    /// Cho phép xem lùi 30 ngày (follow-up no-show) và tới 30 ngày (theo MaxDaysAhead booking).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LichTheoNgay(DateTime? date)
    {
        var picked = (date ?? DateTime.Today).Date;
        var minDate = DateTime.Today.AddDays(-30);
        var maxDate = DateTime.Today.AddDays(Constants.MaxDaysAhead);
        // Clamp ngày được chọn vào range cho phép — chống lễ tân điều hướng tay vào ngày quá xa.
        if (picked < minDate) picked = minDate;
        if (picked > maxDate) picked = maxDate;

        var d = DateOnly.FromDateTime(picked);
        var list = await _apptService.GetByDateAsync(d, CurrentSiteId);

        ViewBag.Title           = $"Lịch ngày {picked:dd/MM/yyyy}";
        ViewBag.PickedDate      = picked;
        ViewBag.MinDate         = minDate;
        ViewBag.MaxDate         = maxDate;
        ViewBag.IsToday         = picked == DateTime.Today;
        ViewBag.TotalCount      = list.Count;
        ViewBag.PendingCount    = list.Count(a => a.Status == Constants.ApptPending);
        ViewBag.ConfirmedCount  = list.Count(a => a.Status == Constants.ApptConfirmed);
        ViewBag.CheckedInCount  = list.Count(a => a.CheckedIn && a.Status != Constants.ApptCancelled && a.Status != Constants.ApptRejected);
        ViewBag.CompletedCount  = list.Count(a => a.Status == Constants.ApptCompleted);
        ViewBag.CancelledCount  = list.Count(a => a.Status == Constants.ApptCancelled || a.Status == Constants.ApptRejected);
        return View(list);
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var a = await _apptService.GetByIdAsync(id);
        if (a == null) return NotFound();
        // Site scoping — chống IDOR cross-site (lễ tân site A không xem được lịch site B)
        if (a.SiteId != CurrentSiteId) return NotFound();
        ViewBag.Title = "Chi tiết lịch hẹn";

        // Load list bác sĩ phù hợp để phân lịch:
        // ưu tiên bs cùng khoa, fallback toàn bộ bs site nếu chưa có ai theo khoa.
        var allDoctors = await _doctorService.GetAllAsync(CurrentSiteId);
        var deptDoctors = a.DepartmentId.HasValue
            ? allDoctors.Where(d => d.DepartmentId == a.DepartmentId.Value).ToList()
            : allDoctors;
        ViewBag.AvailableDoctors = deptDoctors.Any() ? deptDoctors : allDoctors;
        ViewBag.AllDoctorsCount  = allDoctors.Count;
        return View(a);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignDoctor(Guid id, Guid? doctorId)
    {
        var u = CurrentUser!;
        // Site scoping — chặn assign chéo site
        var current = await _apptService.GetByIdAsync(id);
        if (current == null || current.SiteId != CurrentSiteId) return NotFound();
        // Audit detail: ghi cả BS cũ + BS mới để trace handoff
        var oldDoctorName = current.DoctorId.HasValue
            ? (await _doctorService.GetByIdAsync(current.DoctorId.Value))?.NameL
            : null;
        var newDoctorName = doctorId.HasValue
            ? (await _doctorService.GetByIdAsync(doctorId.Value))?.NameL
            : null;
        var ok = await _apptService.AssignDoctorAsync(id, doctorId, u.Id);
        if (ok)
        {
            await _auditService.LogAsync(u.Id, "Phân bác sĩ",
                $"{u.UserName} apptId={id} BN={current.PatientName}: {oldDoctorName ?? "(chưa phân)"} → {newDoctorName ?? "(huỷ phân)"}");
            TempData["Success"] = doctorId.HasValue
                ? "Đã phân bác sĩ phụ trách."
                : "Đã bỏ phân bác sĩ.";
        }
        else
        {
            TempData["Error"] = "Không thể phân bác sĩ — lịch đã hoàn tất hoặc không tồn tại.";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string newStatus, string? staffNote)
    {
        var u = CurrentUser!;
        // Site scoping — không cho lễ tân site A đụng lịch site B
        var current = await _apptService.GetByIdAsync(id);
        if (current == null || current.SiteId != CurrentSiteId) return NotFound();
        var result = await _apptService.UpdateStatusAsync(id, newStatus, staffNote, u.Id);
        if (result.Success)
        {
            // Audit trail đầy đủ: ai, gì (old→new), khi, lý do (nếu có)
            var detail = $"{u.UserName} chuyển lịch {id}: {result.OldStatus} → {result.NewStatus}"
                       + (string.IsNullOrEmpty(result.BookingCode) ? "" : $" (code={result.BookingCode})")
                       + (string.IsNullOrWhiteSpace(staffNote) ? "" : $" | reason: {staffNote.Trim()}");
            await _auditService.LogAsync(u.Id, "Cập nhật lịch hẹn", detail);
            TempData["Success"] = newStatus switch
            {
                Constants.ApptConfirmed => $"Đã duyệt. Mã khám: {result.BookingCode}",
                Constants.ApptRejected  => "Đã từ chối lịch hẹn — bệnh nhân sẽ nhận thông báo.",
                Constants.ApptCancelled => "Đã huỷ lịch hẹn.",
                Constants.ApptCompleted => "Đã đóng lịch (hoàn tất).",
                _                       => "Đã cập nhật trạng thái lịch hẹn."
            };
        }
        else
        {
            TempData["Error"] = result.ErrorMessage ?? "Không thể cập nhật.";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> CheckIn(string? code)
    {
        ViewBag.Title = "Check-in bệnh nhân";
        ViewBag.LookupCode = code;
        ViewBag.LookupResult = string.IsNullOrEmpty(code) ? null : await _apptService.GetByBookingCodeAsync(code);
        ViewBag.TodaysList = await _apptService.GetTodayConfirmedAsync(CurrentSiteId);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkCheckedIn(Guid id, string code)
    {
        var u = CurrentUser!;
        // Site scoping — chặn check-in chéo site
        var current = await _apptService.GetByIdAsync(id);
        if (current == null || current.SiteId != CurrentSiteId) return NotFound();
        var ok = await _apptService.MarkCheckedInAsync(id, u.Id);
        if (ok)
        {
            await _auditService.LogAsync(u.Id, "Check-in bệnh nhân", $"{u.UserName} check-in lịch {code}");
            TempData["Success"] = $"Đã check-in {code}.";
        }
        else
        {
            TempData["Error"] = "Không thể check-in (lịch chưa confirmed hoặc không phải hôm nay).";
        }
        return RedirectToAction(nameof(CheckIn), new { code });
    }

    [HttpGet]
    public async Task<IActionResult> DoctorsOnDuty()
    {
        ViewBag.Title = "Bác sĩ trực hôm nay";
        var today = (byte)DateTime.Today.DayOfWeek;
        var allSchedules = await _scheduleService.GetAllActiveAsync();
        ViewBag.Schedules = allSchedules.Where(s => s.Weekday == today).ToList();
        ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Quotas(DateTime? from, DateTime? to, Guid? departmentId, string? session)
    {
        ViewBag.Title = "Quản lý suất khám";
        var fromD = DateOnly.FromDateTime(from ?? DateTime.Today);
        var toD = DateOnly.FromDateTime(to ?? DateTime.Today.AddDays(7));
        ViewBag.From = fromD;
        ViewBag.To = toD;
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        ViewBag.SelectedDeptId = departmentId;
        ViewBag.SelectedSession = session;

        var list = await _quotaService.GetByDateRangeAsync(fromD, toD);
        // Filter theo khoa + buổi nếu có
        if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            list = list.Where(q => q.DepartmentId == departmentId.Value).ToList();
        if (!string.IsNullOrWhiteSpace(session))
            list = list.Where(q => q.Session == session).ToList();
        // Sắp xếp: Ngày desc, Khoa asc, Buổi (sáng → chiều)
        list = list
            .OrderByDescending(q => q.ApptDate)
            .ThenBy(q => q.DepartmentId.HasValue
                ? ((List<Data.Entities.Department>)ViewBag.Departments)
                    .FirstOrDefault(d => d.Id == q.DepartmentId.Value)?.NameL ?? ""
                : "")
            .ThenBy(q => q.Session == Constants.SessionMorning ? 0 : 1)
            .ToList();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetQuotaMax(Guid departmentId, DateTime date, string session, int max)
    {
        var u = CurrentUser!;
        var d = DateOnly.FromDateTime(date);
        await _quotaService.SetMaxAsync(departmentId, d, session, max);
        await _auditService.LogAsync(u.Id, "Đặt quota lễ tân",
            $"{u.UserName} dept={departmentId} {d} {session} max={max}");
        TempData["Success"] = $"Đã đặt suất {max} cho {d:dd/MM/yyyy} {session}.";
        return RedirectToAction(nameof(Quotas));
    }
}
