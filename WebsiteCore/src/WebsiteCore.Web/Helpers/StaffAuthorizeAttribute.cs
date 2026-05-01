using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebsiteCore.Business;
using WebsiteCore.Business.ViewModels;

namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Custom filter: chỉ cho user thuộc một trong các group được phép.
/// Nếu chưa login → redirect /AdminCP/Login.
/// Nếu login nhưng sai role → 403 hoặc redirect về portal đúng.
///
/// Cách dùng:
///   [StaffAuthorize("RECEPTION")]
///   public class LeTanController : BaseController { ... }
///
///   [StaffAuthorize("DOCTOR")]
///   public class DoctorPortalController : BaseController { ... }
///
///   [StaffAuthorize("ADMIN")]
///   public class DepartmentsController : BaseController { ... }
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class StaffAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedGroups;

    public StaffAuthorizeAttribute(params string[] allowedGroups)
    {
        _allowedGroups = allowedGroups.Length == 0
            ? new[] { Constants.AdminGroup, Constants.DoctorGroup, Constants.ReceptionGroup }
            : allowedGroups;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var s = session.GetString(Constants.UserSession);
        if (string.IsNullOrEmpty(s))
        {
            context.Result = new RedirectResult("~/AdminCP/Login");
            return;
        }
        var user = System.Text.Json.JsonSerializer.Deserialize<LoggedInUser>(s);
        if (user == null)
        {
            context.Result = new RedirectResult("~/AdminCP/Login");
            return;
        }

        // ADMIN bypass mọi role check (super-user)
        if (user.GroupId == Constants.AdminGroup)
            return;

        if (!_allowedGroups.Contains(user.GroupId))
        {
            // Sai role → đẩy về portal đúng của họ
            string redir = user.GroupId switch
            {
                Constants.DoctorGroup    => "~/bac-si-portal",
                Constants.ReceptionGroup => "~/le-tan",
                Constants.MemberGroup    => "~/ho-so",
                _                        => "~/"
            };
            context.Result = new RedirectResult(redir);
        }
    }
}
