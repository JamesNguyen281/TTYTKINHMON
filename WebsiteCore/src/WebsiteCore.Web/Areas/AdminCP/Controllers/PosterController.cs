using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

/// <summary>
/// PosterController = redirect tới NewsController, scope theo user.
/// POSTER là role chỉ được đăng / sửa bài tin tức của chính mình.
/// </summary>
[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup, Constants.PosterGroup)]
public class PosterController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "News", new { area = "AdminCP", mine = true });
}
