using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Hồ sơ cá nhân cho cán bộ (ADMIN / DOCTOR / RECEPTION).
/// Phân biệt với /ho-so dành cho MEMBER (bệnh nhân — có hồ sơ y tế, lịch sử khám, đặt lịch...).
/// Staff profile chỉ có: định danh, liên hệ, đổi mật khẩu, audit info — KHÔNG hiển thị
/// các phần y tế / đặt lịch / lịch sử khám của bệnh nhân.
/// </summary>
[Route("staff-profile")]
public class StaffProfileController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public StaffProfileController(
        ISiteService siteService,
        IUserService userService,
        IAuditService auditService) : base(siteService)
    {
        _userService = userService;
        _auditService = auditService;
    }

    private static bool IsStaff(string? group) =>
        group == Constants.AdminGroup || group == Constants.DoctorGroup || group == Constants.ReceptionGroup;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~" + PortalUrls.StaffLogin);
        if (!IsStaff(u.GroupId)) return Redirect("~" + PortalUrls.MemberHome);

        var fullUser = await _userService.GetByIdAsync(u.Id);
        ViewBag.Title = "Hồ sơ cá nhân";
        return View(fullUser);
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string? FullName, string? Phone, string? Email, int? Gender)
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~" + PortalUrls.StaffLogin);
        if (!IsStaff(u.GroupId)) return Redirect("~" + PortalUrls.MemberHome);

        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Phone))
        {
            TempData["Error"] = "Họ tên và SĐT không được trống.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await _userService.UpdateProfileAsync(u.Id, FullName.Trim(), Phone.Trim(), Email?.Trim(), Gender);
        if (ok)
        {
            await _auditService.LogAsync(u.Id, "Cập nhật hồ sơ cán bộ",
                $"{u.UserName} ({u.GroupId}) cập nhật profile cá nhân");
            // refresh session
            var fresh = await _userService.GetByIdAsync(u.Id);
            if (fresh != null)
            {
                HttpContext.Session.SetObject(Constants.UserSession, new LoggedInUser
                {
                    Id = fresh.Id,
                    UserName = fresh.UserName ?? "",
                    FullName = fresh.FullName,
                    Email = fresh.Email,
                    Phone = fresh.Phone,
                    GroupId = fresh.GroupId ?? "",
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
}
