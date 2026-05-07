using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

/// <summary>
/// Hồ sơ khám bệnh — chỉ ADMIN. Bác sĩ xem hồ sơ chính mình ký qua /bac-si-portal/ho-so-da-kham.
/// </summary>
[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class MedicalRecordsController : BaseController
{
    private readonly TtytlpDbContext _db;
    private readonly IAuditService _audit;

    public MedicalRecordsController(ISiteService siteService, TtytlpDbContext db, IAuditService audit) : base(siteService)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Title = "Hồ sơ khám bệnh";
        // M3: site scoping qua appointment.site_id (MedicalRecord không có cột site_id)
        var siteId = CurrentSiteId;
        var query = from m in _db.MedicalRecords
                    where m.ActiveFlag == 1
                    join a in _db.Appointments on m.AppointmentId equals a.Id into ag
                    from a in ag.DefaultIfEmpty()
                    where a == null || a.SiteId == siteId
                    select m;
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => m.RecordNo!.Contains(q) || m.Diagnosis!.Contains(q));
        var list = await query.OrderByDescending(m => m.VisitDate).Take(100).ToListAsync();
        ViewBag.Q = q;
        return View(list);
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var m = await _db.MedicalRecords.FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return NotFound();
        // M3: chặn cross-site
        if (m.AppointmentId.HasValue)
        {
            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == m.AppointmentId.Value);
            if (appt != null && appt.SiteId != CurrentSiteId) return NotFound();
        }
        ViewBag.Title = "Chi tiết hồ sơ " + m.RecordNo;
        ViewBag.Prescriptions = await _db.Prescriptions.Where(p => p.MedicalRecordId == id).ToListAsync();
        return View(m);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [StaffAuthorize(Constants.AdminGroup)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var m = await _db.MedicalRecords.FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return NotFound();
        if (m.AppointmentId.HasValue)
        {
            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == m.AppointmentId.Value);
            if (appt != null && appt.SiteId != CurrentSiteId) return NotFound();
        }
        m.ActiveFlag = 0;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá hồ sơ khám",
            $"{u.UserName} xoá (ẩn) HS {m.RecordNo} — chẩn đoán: {m.Diagnosis}");
        TempData["Success"] = $"Đã xoá hồ sơ {m.RecordNo}.";
        return RedirectToAction(nameof(Index));
    }
}
