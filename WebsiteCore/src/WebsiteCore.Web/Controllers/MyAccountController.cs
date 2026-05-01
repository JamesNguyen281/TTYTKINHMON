using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>Trang cá nhân của bệnh nhân: hồ sơ, lịch sử khám, đổi profile, đổi mật khẩu.</summary>
public class MyAccountController : BaseController
{
    private readonly IMedicalRecordService _mrService;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public MyAccountController(
        ISiteService siteService,
        IMedicalRecordService mrService,
        IUserService userService,
        IAuditService auditService) : base(siteService)
    {
        _mrService = mrService;
        _userService = userService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/dang-nhap?returnUrl=/ho-so");
        // Cán bộ (ADMIN/DOCTOR/RECEPTION) → trang hồ sơ riêng, không xem giao diện bệnh nhân
        if (u.GroupId == Constants.AdminGroup
            || u.GroupId == Constants.DoctorGroup
            || u.GroupId == Constants.ReceptionGroup)
        {
            return Redirect("~/staff-profile");
        }
        var fullUser = await _userService.GetByIdAsync(u.Id);
        ViewBag.Title = "Hồ sơ của tôi";
        return View(fullUser);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(PatientProfileInput input)
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/dang-nhap");
        if (string.IsNullOrWhiteSpace(input.FullName) || string.IsNullOrWhiteSpace(input.Phone))
        {
            TempData["Error"] = "Họ tên và SĐT không được trống.";
            return RedirectToAction(nameof(Index));
        }
        var ok = await _userService.UpdatePatientProfileAsync(u.Id, input);
        if (ok)
        {
            await _auditService.LogAsync(u.Id, "Cập nhật hồ sơ", $"{u.UserName} đổi thông tin cá nhân");
            var fresh = await _userService.GetByIdAsync(u.Id);
            if (fresh != null)
            {
                HttpContext.Session.SetObject(Constants.UserSession, new LoggedInUser
                {
                    Id = fresh.Id, UserName = fresh.UserName ?? "", FullName = fresh.FullName,
                    Email = fresh.Email, Phone = fresh.Phone, GroupId = fresh.GroupId ?? "",
                    DoctorId = fresh.DoctorId
                });
            }
            TempData["Success"] = "Đã cập nhật hồ sơ.";
        }
        else
        {
            TempData["Error"] = "Không cập nhật được.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/dang-nhap");
        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu mới và xác nhận không khớp.";
            return RedirectToAction(nameof(Index));
        }
        var strengthErr = WebsiteCore.Business.Helpers.StringHelper.ValidatePasswordStrength(newPassword);
        if (strengthErr != null)
        {
            TempData["Error"] = strengthErr;
            return RedirectToAction(nameof(Index));
        }
        if (newPassword == currentPassword)
        {
            TempData["Error"] = "Mật khẩu mới phải khác mật khẩu hiện tại.";
            return RedirectToAction(nameof(Index));
        }
        var ok = await _userService.ChangePasswordAsync(u.Id, currentPassword, newPassword);
        if (ok)
        {
            await _auditService.LogAsync(u.Id, "Đổi mật khẩu", $"{u.UserName} tự đổi mật khẩu (self-service)");
            // L5: invalidate session hiện tại — buộc đăng nhập lại với pwd mới
            HttpContext.Session.Clear();
            TempData["Success"] = "Đã đổi mật khẩu — vui lòng đăng nhập lại với mật khẩu mới.";
            return Redirect("~/dang-nhap");
        }
        TempData["Error"] = "Mật khẩu hiện tại không đúng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> LichSuKham()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/dang-nhap?returnUrl=/lich-su-kham");
        ViewBag.Title = "Lịch sử khám bệnh";
        var records = await _mrService.GetByPatientAsync(u.Id);
        return View(records);
    }
}
