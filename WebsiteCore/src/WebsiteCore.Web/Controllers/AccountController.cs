using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Helpers;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Trang đổi mật khẩu bắt buộc — hiện ra khi user đăng nhập với mật khẩu mặc định "123456".
/// Áp dụng cho mọi role TRỪ ADMIN (admin có thể tự đổi qua AdminCP/Users/ChangePassword).
/// </summary>
public class AccountController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuditService _audit;

    public AccountController(ISiteService siteService, IUserService userService, IAuditService audit)
        : base(siteService)
    {
        _userService = userService;
        _audit = audit;
    }

    [HttpGet]
    [Route("doi-mat-khau")]
    public IActionResult DoiMatKhau()
    {
        if (CurrentUser == null) return Redirect("~/dang-nhap");
        ViewBag.Title = "Đổi mật khẩu";
        ViewBag.PolicyHint = StringHelper.PasswordPolicyHint;
        ViewBag.IsForced = HttpContext.Session.GetString(Constants.ForcePwdChange) == "1";
        ViewData["HideHero"] = true;
        return View();
    }

    [HttpPost]
    [Route("doi-mat-khau")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(string currentPassword, string newPassword, string confirmPassword)
    {
        if (CurrentUser == null) return Redirect("~/dang-nhap");
        ViewBag.Title = "Đổi mật khẩu";
        ViewBag.PolicyHint = StringHelper.PasswordPolicyHint;
        bool isForced = HttpContext.Session.GetString(Constants.ForcePwdChange) == "1";
        ViewBag.IsForced = isForced;
        ViewData["HideHero"] = true;

        if (string.IsNullOrEmpty(currentPassword))
        {
            ModelState.AddModelError("currentPassword", "Vui lòng nhập mật khẩu hiện tại.");
            return View();
        }

        // Validate strength TRƯỚC khi gọi service
        var strengthErr = StringHelper.ValidatePasswordStrength(newPassword);
        if (strengthErr != null)
        {
            ModelState.AddModelError("newPassword", strengthErr);
            return View();
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp.");
            return View();
        }

        if (newPassword == currentPassword)
        {
            ModelState.AddModelError("newPassword", "Mật khẩu mới phải khác mật khẩu hiện tại.");
            return View();
        }

        var ok = await _userService.ChangePasswordAsync(CurrentUser.Id, currentPassword, newPassword);
        if (!ok)
        {
            ModelState.AddModelError("currentPassword", "Mật khẩu hiện tại không đúng.");
            return View();
        }

        // Clear force-change flag — user đã đổi xong, được phép vào hệ thống bình thường
        HttpContext.Session.Remove(Constants.ForcePwdChange);
        await _audit.LogAsync(CurrentUser.Id, "Đổi mật khẩu",
            isForced ? $"{CurrentUser.UserName} đổi mật khẩu (bắt buộc lần đầu)" : $"{CurrentUser.UserName} tự đổi mật khẩu");

        TempData["Success"] = "Đã đổi mật khẩu. Vui lòng dùng mật khẩu mới cho lần đăng nhập tiếp theo.";
        return Redirect("~" + PortalUrls.HomeFor(CurrentUser.GroupId));
    }
}
