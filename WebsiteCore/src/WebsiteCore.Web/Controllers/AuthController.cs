using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Cổng ĐĂNG NHẬP / ĐĂNG KÝ cho BỆNH NHÂN (MEMBER).
///
/// Cross-portal guard quan trọng:
///   - Nếu user tồn tại nhưng group != MEMBER → trả "Tài khoản không tồn tại"
///     (giống user thật sự không tồn tại) → chống enumeration.
///   - Audit log GHI rõ chi tiết internal "Đăng nhập SAI CỔNG" để admin theo dõi.
/// </summary>
public class AuthController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public AuthController(
        ISiteService siteService,
        IUserService userService,
        IAuditService auditService) : base(siteService)
    {
        _userService = userService;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        if (CurrentUser != null) return RedirectByRole();
        ViewBag.Title = "Đăng nhập";
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(LoginViewModel vm)
    {
        ViewBag.Title = "Đăng nhập";
        if (!ModelState.IsValid) return View(vm);

        var u = await _userService.CheckLoginAsync(vm.UserName.Trim(), vm.Password);
        if (u == null)
        {
            // H2: chống username enumeration — chung 1 message cho user-not-found vs wrong-password.
            // Attacker không phân biệt được tài khoản tồn tại hay không.
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
            return View(vm);
        }

        // CROSS-PORTAL GUARD: trang này CHỈ dành cho MEMBER.
        // Cán bộ y tế cố login ở đây → trả "Tài khoản không tồn tại" + log internal.
        if (u.GroupId != Constants.MemberGroup)
        {
            await _auditService.LogAsync(u.Id, "Đăng nhập SAI CỔNG",
                $"{u.UserName} (group={u.GroupId}) cố đăng nhập tại /dang-nhap (cổng bệnh nhân) — đã chặn");
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
            return View(vm);
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
        // L6: log IP + User-Agent cho forensic
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
        var ua = Request.Headers.UserAgent.ToString();
        if (ua.Length > 200) ua = ua.Substring(0, 200);
        await _auditService.LogAsync(u.Id, "Đăng nhập",
            $"{u.UserName} đăng nhập từ cổng bệnh nhân | IP={ip} | UA={ua}");

        // Force đổi mật khẩu nếu đang dùng mật khẩu mặc định "123456" (áp dụng cho mọi role MEMBER)
        if (WebsiteCore.Business.Helpers.StringHelper.IsDefaultPassword(u.Password))
        {
            HttpContext.Session.SetString(Constants.ForcePwdChange, "1");
            TempData["Warning"] = "Bạn đang dùng mật khẩu mặc định. Vui lòng đổi mật khẩu mới để bảo mật.";
            return Redirect("~/doi-mat-khau");
        }

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);
        return Redirect("~/ho-so");
    }

    [HttpGet]
    public IActionResult DangKy()
    {
        if (CurrentUser != null) return RedirectByRole();
        ViewBag.Title = "Đăng ký tài khoản bệnh nhân";
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(RegisterViewModel vm)
    {
        ViewBag.Title = "Đăng ký tài khoản bệnh nhân";
        if (!ModelState.IsValid) return View(vm);

        if (await _userService.UserNameExistsAsync(vm.UserName.Trim().ToLower()))
        {
            ModelState.AddModelError(nameof(vm.UserName), "Tên đăng nhập đã được sử dụng.");
            return View(vm);
        }

        var newId = await _userService.RegisterMemberAsync(vm);
        if (newId == null)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản. Vui lòng thử lại.");
            return View(vm);
        }

        var u = await _userService.GetByIdAsync(newId.Value);
        if (u != null)
        {
            HttpContext.Session.SetObject(Constants.UserSession, new LoggedInUser
            {
                Id = u.Id, UserName = u.UserName, FullName = u.FullName,
                Email = u.Email, Phone = u.Phone, GroupId = u.GroupId
            });
            await _auditService.LogAsync(u.Id, "Đăng ký", $"{u.UserName} tự đăng ký tài khoản bệnh nhân");
        }

        TempData["Success"] = "Tạo tài khoản thành công! Bạn đã được đăng nhập.";
        return Redirect("~/ho-so");
    }

    [HttpGet]
    public async Task<IActionResult> DangXuat()
    {
        var u = CurrentUser;
        if (u != null)
        {
            await _auditService.LogAsync(u.Id, "Đăng xuất", $"{u.UserName} đăng xuất");
        }
        var grp = u?.GroupId;
        HttpContext.Session.Clear();

        // MEMBER (hoặc anon) → trang chủ. Cán bộ → /AdminCP/Login.
        if (grp == Constants.AdminGroup || grp == Constants.DoctorGroup || grp == Constants.ReceptionGroup)
            return Redirect("~" + PortalUrls.StaffLogin);
        return Redirect("~/");
    }

    private IActionResult RedirectByRole()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/");
        return Redirect("~" + PortalUrls.HomeFor(u.GroupId));
    }
}
