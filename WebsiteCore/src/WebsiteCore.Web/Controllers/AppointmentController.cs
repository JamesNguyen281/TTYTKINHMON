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
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
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
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
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
        return Redirect(u != null ? "~/lich-cua-toi" : "~/dat-lich-kham");
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
