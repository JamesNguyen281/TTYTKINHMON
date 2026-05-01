using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup, Constants.ReceptionGroup)]
public class QuotasController : BaseController
{
    private readonly IQuotaService _service;
    private readonly IDepartmentService _deptService;
    private readonly IAuditService _audit;

    public QuotasController(
        ISiteService siteService,
        IQuotaService service,
        IDepartmentService deptService,
        IAuditService audit) : base(siteService)
    {
        _service = service;
        _deptService = deptService;
        _audit = audit;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? mode)
    {
        ViewBag.Title = "Quản lý suất khám";

        DateTime f, t;
        switch (mode)
        {
            case "day":   f = DateTime.Today; t = DateTime.Today; break;
            case "3days": f = DateTime.Today; t = DateTime.Today.AddDays(2); break;
            case "week":  f = DateTime.Today; t = DateTime.Today.AddDays(6); break;
            case "month": f = DateTime.Today; t = DateTime.Today.AddDays(29); break;
            default:      f = from ?? DateTime.Today; t = to ?? DateTime.Today.AddDays(6); break;
        }
        if (t < f) t = f;

        var fromD = DateOnly.FromDateTime(f);
        var toD   = DateOnly.FromDateTime(t);

        var depts  = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        var quotas = await _service.GetByDateRangeAsync(fromD, toD);

        var dates = new List<DateOnly>();
        for (var d = fromD; d <= toD; d = d.AddDays(1)) dates.Add(d);

        // Lookup (deptId, date, session) -> quota để view dễ truy cell
        var matrix = quotas
            .Where(q => q.DepartmentId.HasValue && !string.IsNullOrEmpty(q.Session))
            .GroupBy(q => (DeptId: q.DepartmentId!.Value, Date: q.ApptDate, Session: q.Session!))
            .ToDictionary(g => g.Key, g => g.First());

        ViewBag.Mode = mode ?? "week";
        ViewBag.From = fromD;
        ViewBag.To = toD;
        ViewBag.Departments = depts;
        ViewBag.Dates = dates;
        ViewBag.Matrix = matrix;
        ViewBag.DefaultMax = Constants.DefaultQuotaPerSession;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMax(Guid departmentId, DateTime date, string session, int max)
    {
        var d = DateOnly.FromDateTime(date);
        await _service.SetMaxAsync(departmentId, d, session, max);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Đặt quota", $"{u.UserName} dept={departmentId} {d} {session} max={max}");
        TempData["Success"] = $"Đã đặt suất {max} cho {d:dd/MM/yyyy} {session}.";
        return RedirectToAction(nameof(Index));
    }
}
