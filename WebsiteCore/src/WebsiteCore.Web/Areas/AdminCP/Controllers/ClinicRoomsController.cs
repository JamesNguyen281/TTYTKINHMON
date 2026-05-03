using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data.Entities;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

/// <summary>
/// AdminCP — quản lý ClinicRoom (phòng khám trong Khoa Khám bệnh).
/// Workflow: admin tạo room cho từng chuyên khoa, gán BS qua DoctorSchedule.ClinicRoomId,
/// admin có thể deactivate room khi không còn dùng (soft-delete giữ tham chiếu lịch sử).
/// </summary>
[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class ClinicRoomsController : BaseController
{
    private readonly IClinicRoomService _service;
    private readonly IDepartmentService _deptService;
    private readonly IAuditService _auditService;

    public ClinicRoomsController(
        ISiteService siteService,
        IClinicRoomService service,
        IDepartmentService deptService,
        IAuditService auditService) : base(siteService)
    {
        _service = service;
        _deptService = deptService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Quản lý phòng khám";
        var list = await _service.GetActiveBySiteAsync(CurrentSiteId);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Tạo phòng khám mới";
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View(new ClinicRoom { ActiveFlag = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClinicRoom room)
    {
        if (string.IsNullOrWhiteSpace(room.RoomCode) || string.IsNullOrWhiteSpace(room.RoomName))
        {
            TempData["Error"] = "Mã phòng và tên phòng không được trống.";
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View(room);
        }
        // Cross-site guard: dept phải cùng site với admin
        var dept = await _deptService.GetByIdAsync(room.DepartmentId);
        if (dept == null || dept.SiteId != CurrentSiteId)
        {
            TempData["Error"] = "Khoa không hợp lệ hoặc khác cơ sở.";
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View(room);
        }
        var u = CurrentUser!;
        room.CreatedBy = u.Id;
        try
        {
            await _service.CreateAsync(room);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tạo phòng: " + ex.Message;
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View(room);
        }
        await _auditService.LogAsync(u.Id, "Tạo phòng khám",
            $"{u.UserName} tạo {room.RoomCode} - {room.RoomName} (dept: {dept.NameL})");
        TempData["Success"] = $"Đã tạo phòng {room.RoomCode}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var room = await _service.GetByIdInSiteAsync(id, CurrentSiteId);
        if (room == null) return NotFound();
        ViewBag.Title = "Sửa phòng khám: " + room.RoomName;
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClinicRoom room)
    {
        var existing = await _service.GetByIdInSiteAsync(room.Id, CurrentSiteId);
        if (existing == null) return NotFound();
        // Cross-site guard cho dept mới (admin có thể đổi dept)
        var dept = await _deptService.GetByIdAsync(room.DepartmentId);
        if (dept == null || dept.SiteId != CurrentSiteId)
        {
            TempData["Error"] = "Khoa không hợp lệ.";
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View(room);
        }
        var u = CurrentUser!;
        room.LuUserId = u.Id;
        await _service.UpdateAsync(room);
        await _auditService.LogAsync(u.Id, "Sửa phòng khám",
            $"{u.UserName} sửa {room.RoomCode} - {room.RoomName}");
        TempData["Success"] = "Đã cập nhật phòng khám.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var u = CurrentUser!;
        var ok = await _service.DeleteAsync(id, CurrentSiteId);
        if (ok)
        {
            await _auditService.LogAsync(u.Id, "Vô hiệu hoá phòng khám",
                $"{u.UserName} vô hiệu hoá phòng id={id}");
            TempData["Success"] = "Đã vô hiệu hoá phòng khám (soft-delete giữ history).";
        }
        else
        {
            TempData["Error"] = "Không thể vô hiệu — phòng không tồn tại hoặc khác cơ sở.";
        }
        return RedirectToAction(nameof(Index));
    }
}
