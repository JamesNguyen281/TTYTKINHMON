using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;
using WebsiteCore.Web.Controllers;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Areas.AdminCP.Controllers;

[Area("AdminCP")]
[StaffAuthorize(Constants.AdminGroup)]
public class UsersController : BaseController
{
    private readonly TtytlpDbContext _db;
    private readonly IAuditService _auditService;

    public UsersController(ISiteService siteService, TtytlpDbContext db, IAuditService auditService)
        : base(siteService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(string? q, string? group)
    {
        ViewBag.Title = "Quản lý người dùng";
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.UserName!.Contains(q) || u.FullName!.Contains(q) || u.Phone!.Contains(q));
        if (!string.IsNullOrWhiteSpace(group))
            query = query.Where(u => u.GroupId == group);
        var list = await query.OrderByDescending(u => u.CreatedDate).Take(200).ToListAsync();
        ViewBag.Q = q;
        ViewBag.Group = group;
        return View(list);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound();
        ViewBag.Title = "Sửa user: " + u.UserName;
        return View(u);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, string? FullName, string? Email, string? Phone, string? GroupId, int ActiveFlag)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound();
        u.FullName = FullName;
        u.Email = Email;
        u.Phone = Phone;
        u.GroupId = GroupId;
        u.ActiveFlag = ActiveFlag;
        u.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var staff = CurrentUser!;
        await _auditService.LogAsync(staff.Id, "Sửa user", $"{staff.UserName} sửa user {u.UserName} (group={u.GroupId})");
        TempData["Success"] = "Đã cập nhật user.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Thêm người dùng";
        return View(new User { ActiveFlag = 1, GroupId = "MEMBER" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string UserName, string Password, string? FullName, string? Email, string? Phone, string? GroupId, int ActiveFlag)
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            TempData["Error"] = "Username và mật khẩu là bắt buộc.";
            return RedirectToAction(nameof(Create));
        }
        var uname = UserName.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.UserName == uname))
        {
            TempData["Error"] = "Username đã tồn tại.";
            return RedirectToAction(nameof(Create));
        }
        var u = new User
        {
            Id = Guid.NewGuid(),
            UserName = uname,
            Password = WebsiteCore.Business.Helpers.StringHelper.HashPassword(Password),
            FullName = FullName,
            Email = Email,
            Phone = Phone,
            GroupId = string.IsNullOrWhiteSpace(GroupId) ? "MEMBER" : GroupId,
            ActiveFlag = ActiveFlag,
            CreatedDate = DateTime.Now,
            ImagePath = "assets/client/images/doctor-default.svg"
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        var staff = CurrentUser!;
        await _auditService.LogAsync(staff.Id, "Tạo user", $"{staff.UserName} tạo user {u.UserName} (group={u.GroupId})");
        TempData["Success"] = "Đã tạo user mới.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(Guid id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound();
        ViewBag.Title = "Đổi mật khẩu: " + u.UserName;
        return View(u);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(Guid id, string newPassword, string confirmPassword)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound();
        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu xác nhận không khớp.";
            return RedirectToAction(nameof(ChangePassword), new { id });
        }
        // H6: enforce password strength (đồng bộ chính sách user tự đổi qua /ho-so)
        var strengthErr = WebsiteCore.Business.Helpers.StringHelper.ValidatePasswordStrength(newPassword);
        if (strengthErr != null)
        {
            TempData["Error"] = strengthErr;
            return RedirectToAction(nameof(ChangePassword), new { id });
        }
        u.Password = WebsiteCore.Business.Helpers.StringHelper.HashPassword(newPassword);
        u.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var staff = CurrentUser!;
        await _auditService.LogAsync(staff.Id, "Đổi mật khẩu user", $"{staff.UserName} đặt lại mật khẩu cho {u.UserName}");
        TempData["Success"] = $"Đã đặt lại mật khẩu cho {u.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(Guid id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound();
        u.ActiveFlag = u.ActiveFlag == 1 ? 0 : 1;
        u.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var staff = CurrentUser!;
        await _auditService.LogAsync(staff.Id, u.ActiveFlag == 1 ? "Mở khoá user" : "Khoá user",
            $"{staff.UserName} {(u.ActiveFlag == 1 ? "mở khoá" : "khoá")} {u.UserName}");
        TempData["Success"] = u.ActiveFlag == 1 ? "Đã mở khoá user." : "Đã khoá user.";
        return RedirectToAction(nameof(Index));
    }
}
