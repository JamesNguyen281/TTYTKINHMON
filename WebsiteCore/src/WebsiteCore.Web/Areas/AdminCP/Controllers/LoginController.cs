using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

/// <summary>
/// Cổng đăng nhập cán bộ y tế (ADMIN / DOCTOR / RECEPTION).
/// Cross-portal guard: MEMBER cố login ở đây → "Tài khoản không tồn tại".
///
/// Sau login thành công, role-based redirect:
///   ADMIN     → /AdminCP
///   DOCTOR    → /bac-si-portal
///   RECEPTION → /le-tan
/// </summary>
[Area("AdminCP")]
public class LoginController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public LoginController(
        ISiteService siteService,
        IUserService userService,
        IAuditService auditService) : base(siteService)
    {
        _userService = userService;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (CurrentUser != null) return RedirectByRole();
        ViewBag.Title = "Đăng nhập quản trị";
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        ViewBag.Title = "Đăng nhập quản trị";
        if (!ModelState.IsValid) return View("Index", vm);

        var u = await _userService.CheckLoginAsync(vm.UserName.Trim(), vm.Password);
        if (u == null)
        {
            // H2: chống username enumeration — chung 1 message
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng");
            return View("Index", vm);
        }

        // CROSS-PORTAL GUARD: cổng này KHÔNG cho MEMBER login.
        if (u.GroupId == Constants.MemberGroup)
        {
            await _auditService.LogAsync(u.Id, "Đăng nhập SAI CỔNG",
                $"{u.UserName} (group=MEMBER) cố đăng nhập tại /AdminCP/Login (cổng nhân viên) — đã chặn");
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng");
            return View("Index", vm);
        }

        // Set session
        HttpContext.Session.SetObject(Constants.UserSession, new LoggedInUser
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            Email = u.Email,
            Phone = u.Phone,
            GroupId = u.GroupId,
            DoctorId = u.DoctorId
        });
        // L6: log IP + User-Agent
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
        var ua = Request.Headers.UserAgent.ToString();
        if (ua.Length > 200) ua = ua.Substring(0, 200);
        await _auditService.LogAsync(u.Id, "Đăng nhập",
            $"{u.UserName} ({u.GroupId}) đăng nhập AdminCP | IP={ip} | UA={ua}");

        // Force đổi mật khẩu nếu đang dùng mật khẩu mặc định "123456" — TRỪ tài khoản ADMIN
        // (admin có thể tự đổi qua AdminCP/Users/ChangePassword khi muốn)
        if (u.GroupId != Constants.AdminGroup
            && WebsiteCore.Business.Helpers.StringHelper.IsDefaultPassword(u.Password))
        {
            HttpContext.Session.SetString(Constants.ForcePwdChange, "1");
            TempData["Warning"] = "Bạn đang dùng mật khẩu mặc định. Vui lòng đổi mật khẩu mới để bảo mật.";
            return Redirect("~/doi-mat-khau");
        }

        return RedirectByRole();
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var u = CurrentUser;
        if (u != null)
            await _auditService.LogAsync(u.Id, "Đăng xuất", $"{u.UserName} đăng xuất");
        HttpContext.Session.Clear();
        return Redirect("~" + PortalUrls.StaffLogin);
    }

    private IActionResult RedirectByRole()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~" + PortalUrls.StaffLogin);
        return Redirect("~" + PortalUrls.HomeFor(u.GroupId));
    }
}
