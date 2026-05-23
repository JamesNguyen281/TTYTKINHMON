using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class DefaultController : BaseController
{
    private readonly TtytlpDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IAuditService _audit;
    private readonly VisitorCounter _counter;
    private readonly ILogger<DefaultController> _logger;

    public DefaultController(ISiteService siteService, TtytlpDbContext db, IWebHostEnvironment env, IAuditService audit, VisitorCounter counter, ILogger<DefaultController> logger) : base(siteService)
    {
        _db = db;
        _env = env;
        _audit = audit;
        _counter = counter;
        _logger = logger;
    }

    private string NotificationPath  => Path.Combine(_env.WebRootPath, "assets", "admin", "notification", "notification.txt");
    private string Notification2Path => Path.Combine(_env.WebRootPath, "assets", "admin", "notification", "notification2.txt");

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Tổng quan";
        ViewBag.UserCount    = await _db.Users.CountAsync(u => u.ActiveFlag == 1);
        ViewBag.DeptCount    = await _db.Departments.CountAsync(d => d.ActiveFlag == 1);
        ViewBag.NewsCount    = await _db.News.CountAsync(n => n.ActiveFlag == 1);
        ViewBag.ApptCount    = await _db.Appointments.CountAsync();
        ViewBag.PendingAppt  = await _db.Appointments.CountAsync(a => a.Status == Constants.ApptPending);
        ViewBag.QuestionCount      = await _db.Questions.CountAsync();
        ViewBag.MedicalRecordCount = await _db.MedicalRecords.CountAsync(m => m.ActiveFlag == 1);
        ViewBag.DoctorCount        = await _db.Doctors.CountAsync(d => d.ActiveFlag == 1);
        ViewBag.ClinicRoomCount    = await _db.ClinicRooms.CountAsync(c => c.ActiveFlag == 1);

        // Notification banner (file-backed)
        ViewBag.NotificationOn   = System.IO.File.Exists(NotificationPath) && (await System.IO.File.ReadAllTextAsync(NotificationPath)).Trim().Length > 0;
        ViewBag.NotificationText = System.IO.File.Exists(Notification2Path) ? await System.IO.File.ReadAllTextAsync(Notification2Path) : "";

        // Dashboard banner image
        var dashImg = await SiteService.GetDashboardImageAsync(CurrentSiteId);
        if (string.IsNullOrEmpty(dashImg)) dashImg = "/assets/client/images/hero-kinhmon.jpg";
        ViewBag.DashboardImage = dashImg;

        // Visit counter
        ViewBag.OnlineCount = _counter.OnlineCount;
        ViewBag.TodayCount  = _counter.TodayCount;
        ViewBag.TotalCount  = _counter.TotalCount;

        // Comment counts
        ViewBag.UnreadCommentCount = await _db.Comments.CountAsync(c => c.ActiveFlag != 1);
        ViewBag.TotalCommentCount  = await _db.Comments.CountAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetCounter(string which)
    {
        var dir = Path.Combine(_env.WebRootPath, "assets", "admin", "count");
        try
        {
            switch (which)
            {
                case "today":
                    System.IO.File.WriteAllText(Path.Combine(dir, "Count_Vstoday.txt"), DateTime.Now.ToString("dd/MM/yyyy") + "0");
                    break;
                case "total":
                    System.IO.File.WriteAllText(Path.Combine(dir, "Count_Visited.txt"), "0");
                    break;
                case "online":
                    _counter.ResetOnline();
                    break;
                case "all":
                    System.IO.File.WriteAllText(Path.Combine(dir, "Count_Vstoday.txt"), DateTime.Now.ToString("dd/MM/yyyy") + "0");
                    System.IO.File.WriteAllText(Path.Combine(dir, "Count_Visited.txt"), "0");
                    _counter.ResetOnline();
                    break;
            }
            TempData["Success"] = "Đã reset counter.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetCounter failed for command {Which} by user {UserId}", which, CurrentUser?.Id);
            TempData["Error"] = "Không reset được counter — vui lòng thử lại hoặc kiểm tra log.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Turnon(string Command, string TextArea1)
    {
        var u = CurrentUser!;
        // Lưu textarea content vào notification2.txt
        await System.IO.File.WriteAllTextAsync(Notification2Path, TextArea1 ?? "");
        // Bật/tắt: notification.txt = TextArea1 nếu bật, "" nếu tắt
        if (Command == "Bật" || Command == "Bat" || (Command ?? "").ToLower().Contains("on"))
            await System.IO.File.WriteAllTextAsync(NotificationPath, TextArea1 ?? "");
        else
            await System.IO.File.WriteAllTextAsync(NotificationPath, "");
        await _audit.LogAsync(u.Id, "Đổi thông báo", $"{u.UserName} đổi thông báo dashboard ({Command})");
        TempData["Success"] = "Đã cập nhật thông báo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDashboardImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["DashErr"] = "Vui lòng chọn file ảnh.";
            return RedirectToAction(nameof(Index));
        }
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
        {
            TempData["DashErr"] = "File phải là jpg/png/gif/webp.";
            return RedirectToAction(nameof(Index));
        }
        if (file.Length > 20 * 1024 * 1024)
        {
            TempData["DashErr"] = "File vượt quá 20MB.";
            return RedirectToAction(nameof(Index));
        }
        var dir = Path.Combine(_env.WebRootPath, "assets", "admin", "images", "dashboard");
        Directory.CreateDirectory(dir);
        var name = $"dashboard_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
        var fullPath = Path.Combine(dir, name);
        using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }
        var rel = $"/assets/admin/images/dashboard/{name}";
        await SiteService.UpdateDashboardImageAsync(CurrentSiteId, rel);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Đổi ảnh dashboard", $"{u.UserName} upload {name}");
        // Clear site session để layout đọc lại
        HttpContext.Session.Remove(Constants.SiteSession);
        TempData["DashOk"] = "Đã upload và lưu ảnh dashboard mới.";
        return RedirectToAction(nameof(Index));
    }
}
