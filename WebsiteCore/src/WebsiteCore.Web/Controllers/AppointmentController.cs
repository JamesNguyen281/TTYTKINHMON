using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Đặt lịch khám — public, hỗ trợ cả MEMBER (auto-fill) và vãng lai.
/// /lich-cua-toi yêu cầu MEMBER login.
/// </summary>
public class AppointmentController : BaseController
{
    private readonly IDepartmentService _deptService;
    private readonly IAppointmentService _apptService;

    public AppointmentController(
        ISiteService siteService,
        IDepartmentService deptService,
        IAppointmentService apptService) : base(siteService)
    {
        _deptService = deptService;
        _apptService = apptService;
    }

    [HttpGet]
    public async Task<IActionResult> DatLichKham()
    {
        ViewBag.Title = "Đặt lịch khám";
        // P2.D — chỉ hiển thị khoa nhận BN khám (loại Khoa Xét nghiệm/Dược/KS bệnh tật/
        // Khoa Cấp cứu (đến trực tiếp)/Khoa Khám bệnh wrapper)
        ViewBag.Departments = await _deptService.GetClinicalBySiteAsync(CurrentSiteId);
        ViewBag.MaxDaysAhead = Constants.MaxDaysAhead;

        var u = CurrentUser;
        var vm = new BookingInputModel
        {
            PatientName  = u?.FullName ?? string.Empty,
            PatientPhone = u?.Phone    ?? string.Empty,
            PatientEmail = u?.Email,
            AppointmentDate = DateTime.Today.AddDays(1)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatLichKham(BookingInputModel vm)
    {
        ViewBag.Title = "Đặt lịch khám";
        // P2.D — chỉ hiển thị khoa nhận BN khám (loại Khoa Xét nghiệm/Dược/KS bệnh tật/
        // Khoa Cấp cứu (đến trực tiếp)/Khoa Khám bệnh wrapper)
        ViewBag.Departments = await _deptService.GetClinicalBySiteAsync(CurrentSiteId);
        ViewBag.MaxDaysAhead = Constants.MaxDaysAhead;

        if (!ModelState.IsValid) return View(vm);

        var u = CurrentUser;
        var result = await _apptService.CreateAsync(vm, u?.Id, CurrentSiteId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo lịch khám.");
            return View(vm);
        }

        TempData["Success"] = "Đã ghi nhận yêu cầu đặt lịch. Nhân viên Trung tâm sẽ liên hệ xác nhận trong 24 giờ.";
        // Member → /lich-cua-toi (theo dõi realtime). Khách vãng lai → trang xác nhận DaDat đọc
        // apptId từ Session (không truyền qua URL/query) — chống IDOR/guess.
        if (u == null && result.AppointmentId.HasValue)
        {
            HttpContext.Session.SetString("LastAnonBookingId", result.AppointmentId.Value.ToString());
        }
        return u != null
            ? Redirect("~/lich-cua-toi")
            : RedirectToAction(nameof(DaDat));
    }

    /// <summary>
    /// Trang xác nhận đặt lịch cho khách vãng lai. Đọc apptId từ Session (set bởi POST DatLichKham)
    /// — không nhận id từ URL để tránh IDOR. Session expire 30' → khách phải gọi lễ tân tra theo SĐT.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DaDat()
    {
        var idStr = HttpContext.Session.GetString("LastAnonBookingId");
        if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var id))
            return Redirect("~/");
        var a = await _apptService.GetByIdAsync(id);
        if (a == null || a.SiteId != CurrentSiteId) return NotFound();
        ViewBag.Title = "Đã ghi nhận lịch khám";
        ViewBag.HideHero = true;
        return View(a);
    }

    [HttpGet]
    public async Task<IActionResult> LichCuaToi()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/dang-nhap?returnUrl=/lich-cua-toi");

        ViewBag.Title = "Lịch khám của tôi";
        var list = await _apptService.GetByPatientAsync(u.Id);
        return View(list);
    }

    /// <summary>
    /// Endpoint JSON cho polling real-time — trả snapshot tối giản.
    /// Client compare với snapshot ban đầu, nếu khác thì reload trang.
    /// </summary>
    [HttpGet]
    [Route("lich-cua-toi/check-updates")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> CheckUpdates()
    {
        var u = CurrentUser;
        if (u == null) return Json(new { ok = false });

        var list = await _apptService.GetByPatientAsync(u.Id);
        var rows = list.Select(a => new {
            id = a.Id,
            s  = a.Status,
            c  = a.BookingCode,
            n  = a.StaffNote,
            ci = a.CheckedIn
        });
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return Json(new { ok = true, rows });
    }
}
