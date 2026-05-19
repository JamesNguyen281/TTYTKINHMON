using WebsiteCore.Business;

namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Nguồn duy nhất cho URL portal theo role. Mọi nơi cần map group_id → trang home portal
/// (RedirectByRole, header user-link, layout topbar) phải dùng helper này thay vì hard-code
/// switch tự viết — tránh drift khi route portal đổi.
/// </summary>
public static class PortalUrls
{
    public const string StaffLogin  = "/AdminCP/Login";
    public const string StaffLogout = "/AdminCP/Login/Logout";
    public const string MemberLogin = "/dang-nhap";
    public const string MemberHome  = "/ho-so";
    public const string AdminHome   = "/AdminCP/Default";
    public const string DoctorHome  = "/bac-si-portal";
    public const string ReceptionHome = "/le-tan";
    public const string PublicHome  = "/";

    /// <summary>Trang home portal tương ứng với group_id. Group lạ / null → trang chủ public.</summary>
    public static string HomeFor(string? groupId) => groupId switch
    {
        Constants.AdminGroup     => AdminHome,
        Constants.DoctorGroup    => DoctorHome,
        Constants.ReceptionGroup => ReceptionHome,
        Constants.MemberGroup    => MemberHome,
        _                        => PublicHome
    };

    /// <summary>Trang login phù hợp: cán bộ → AdminCP, MEMBER/anon → /dang-nhap.</summary>
    public static string LoginFor(string? groupId) =>
        groupId == Constants.AdminGroup
        || groupId == Constants.DoctorGroup
        || groupId == Constants.ReceptionGroup
            ? StaffLogin
            : MemberLogin;
}
