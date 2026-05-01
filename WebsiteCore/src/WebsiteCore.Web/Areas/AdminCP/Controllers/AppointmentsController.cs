using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup, Constants.ReceptionGroup)]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _service;
    private readonly IAuditService _audit;

    public AppointmentsController(
        ISiteService siteService,
        IAppointmentService service,
        IAuditService audit) : base(siteService)
    {
        _service = service;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string status = "pending")
    {
        ViewBag.Title = "Quản lý lịch khám";
        ViewBag.Status = status;
        var list = await _service.GetByStatusAsync(status, CurrentSiteId);
        // Admin xem hàng đợi: mới nhất lên đầu để thao tác nhanh
        list = list.OrderByDescending(a => a.CreatedDate).ToList();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        var a = await _service.GetByIdAsync(id);
        if (a == null) return NotFound();
        // Site scoping — chống IDOR cross-site (admin site A không xem được lịch site B)
        if (a.SiteId != CurrentSiteId) return NotFound();
        ViewBag.Title = "Chi tiết lịch — " + (a.PatientName ?? "");
        return View(a);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string newStatus, string? staffNote)
    {
        var u = CurrentUser!;
        // Site scoping — chống cross-site tampering
        var existing = await _service.GetByIdAsync(id);
        if (existing == null || existing.SiteId != CurrentSiteId) return NotFound();
        // Xoá flash key của lần trước để tránh leak Success/Error sang request tiếp theo (cùng session, khác appt)
        TempData.Remove("Success");
        TempData.Remove("Error");
        var result = await _service.UpdateStatusAsync(id, newStatus, staffNote, u.Id);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage ?? "Không cập nhật được trạng thái.";
        }
        else
        {
            var detail = $"{u.UserName} chuyển lịch {id}: {result.OldStatus} → {result.NewStatus}"
                       + (string.IsNullOrEmpty(result.BookingCode) ? "" : $" (code={result.BookingCode})")
                       + (string.IsNullOrWhiteSpace(staffNote) ? "" : $" | reason: {staffNote.Trim()}");
            await _audit.LogAsync(u.Id, "Đổi trạng thái lịch", detail);
            TempData["Success"] = result.BookingCode != null ? $"Đã xác nhận. Mã: {result.BookingCode}" : "Đã cập nhật.";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var u = CurrentUser!;
        // Site scoping
        var existing = await _service.GetByIdAsync(id);
        if (existing == null || existing.SiteId != CurrentSiteId) return NotFound();
        TempData.Remove("Success");
        TempData.Remove("Error");
        var ok = await _service.MarkCheckedInAsync(id, u.Id);
        if (ok)
        {
            await _audit.LogAsync(u.Id, "Check-in", $"{u.UserName} check-in lịch ID={id}");
            TempData["Success"] = "Đã check-in.";
        }
        else
        {
            TempData["Error"] = "Không check-in được (lịch không hợp lệ hoặc không phải hôm nay).";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string status = "pending")
    {
        var list = await _service.GetByStatusAsync(status, CurrentSiteId);
        var sb = new System.Text.StringBuilder();
        sb.Append('﻿'); // BOM cho Excel mở UTF-8
        sb.AppendLine("Mã,Họ tên,SĐT,Email,Chuyên khoa,Ngày khám,Buổi,Trạng thái,Đã CheckIn,Tạo lúc,Lý do,Ghi chú NV");
        foreach (var a in list)
        {
            string Esc(string? s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
            sb.AppendLine(string.Join(",",
                Esc(a.BookingCode),
                Esc(a.PatientName),
                Esc(a.PatientPhone),
                Esc(a.PatientEmail),
                Esc(a.DepartmentName),
                Esc(a.AppointmentDate?.ToString("dd/MM/yyyy")),
                Esc(a.Session),
                Esc(a.Status),
                Esc(a.CheckedIn ? "Có" : ""),
                Esc(a.CreatedDate?.ToString("dd/MM/yyyy HH:mm")),
                Esc(a.Reason),
                Esc(a.StaffNote)
            ));
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"lich-kham-{status}-{DateTime.Now:yyyyMMddHHmm}.csv";
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xuất CSV lịch", $"{u.UserName} xuất {list.Count} lịch ({status})");
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }
}
