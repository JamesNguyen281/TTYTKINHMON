using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup, Constants.ReceptionGroup, Constants.DoctorGroup, Constants.PosterGroup)]
public class QrCodeController : BaseController
{
    public QrCodeController(ISiteService siteService) : base(siteService) { }

    public IActionResult Index()
    {
        ViewBag.Title = "Tạo mã QR";
        return View();
    }
}
