using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class ScheduleRequestsController : BaseController
{
    private readonly IScheduleChangeRequestService _scrService;
    private readonly IDoctorService _doctorService;
    private readonly IAuditService _audit;
    private readonly TtytlpDbContext _db;

    public ScheduleRequestsController(
        ISiteService siteService,
        IScheduleChangeRequestService scrService,
        IDoctorService doctorService,
        IAuditService audit,
        TtytlpDbContext db) : base(siteService)
    {
        _scrService = scrService;
        _doctorService = doctorService;
        _audit = audit;
        _db = db;
    }

    public async Task<IActionResult> Index(string? status = "pending")
    {
        ViewBag.Title = "Yêu cầu đổi lịch trực";
        var rawList = await _scrService.GetByStatusAsync(string.IsNullOrEmpty(status) ? null : status);

        // Site scoping — chỉ giữ request của BS thuộc site hiện tại
        // (qua Doctor.DepartmentId → Department.SiteId; BS không có Department coi như public)
        var siteDoctorIds = await (from d in _db.Doctors
                                   join dep in _db.Departments on d.DepartmentId equals dep.Id
                                   where dep.SiteId == CurrentSiteId
                                   select d.Id).ToListAsync();
        var siteDocSet = new HashSet<Guid>(siteDoctorIds);
        var list = rawList.Where(r => siteDocSet.Contains(r.DoctorId)).ToList();

        // Lookup doctor names via User table (Doctor entity có ntext gây SQL parser issues)
        var doctorIds = list.Select(r => r.DoctorId).Distinct().ToList();
        var docNames = new Dictionary<Guid, string>();
        if (doctorIds.Count > 0)
        {
            var users = await _db.Users
                .Where(u => u.DoctorId.HasValue && doctorIds.Contains(u.DoctorId.Value))
                .Select(u => new { u.DoctorId, u.FullName })
                .ToListAsync();
            foreach (var u in users)
                if (u.DoctorId.HasValue) docNames[u.DoctorId.Value] = u.FullName ?? "—";
        }
        ViewBag.DoctorNames = docNames;
        ViewBag.SelectedStatus = status;
        ViewBag.PendingCount = await _scrService.CountPendingAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(Guid id, string action, string? response)
    {
        var u = CurrentUser!;
        TempData.Remove("Success");
        TempData.Remove("Error");
        if (action != "approve" && action != "reject")
        {
            TempData["Error"] = "Action không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }
        var newStatus = action == "approve" ? "approved" : "rejected";
        var ok = await _scrService.ProcessAsync(id, newStatus, response, u.Id);
        if (!ok)
        {
            TempData["Error"] = "Không xử lý được — yêu cầu có thể đã được duyệt trước đó.";
            return RedirectToAction(nameof(Index));
        }
        await _audit.LogAsync(u.Id, "Duyệt yêu cầu đổi lịch",
            $"{u.UserName} {newStatus} request ID={id}{(string.IsNullOrEmpty(response) ? "" : $" | resp: {(response.Length > 100 ? response.Substring(0, 100) + "…" : response)}")}");
        TempData["Success"] = action == "approve"
            ? "Đã duyệt yêu cầu. Vào Lịch trực để cập nhật ca thực tế."
            : "Đã từ chối yêu cầu — bs sẽ thấy phản hồi của bạn.";
        return RedirectToAction(nameof(Index));
    }
}
