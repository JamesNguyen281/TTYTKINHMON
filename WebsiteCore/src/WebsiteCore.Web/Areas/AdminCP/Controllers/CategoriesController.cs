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
public class CategoriesController : BaseController
{
    private readonly ICategoryService _service;
    private readonly IAuditService _audit;
    private readonly TtytlpDbContext _db;

    public CategoriesController(
        ISiteService siteService,
        ICategoryService service,
        IAuditService audit,
        TtytlpDbContext db) : base(siteService)
    {
        _service = service;
        _audit = audit;
        _db = db;
    }

    private static readonly int[] PageSizeOptions = { 10, 20, 50, 100, 200, 500, 1000 };

    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        ViewBag.Title = "Quản lý danh mục";
        if (!PageSizeOptions.Contains(pageSize)) pageSize = 20;
        if (page < 1) page = 1;

        var all = await _service.GetAllAsync(CurrentSiteId);
        var total = all.Count;
        var totalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)pageSize);
        if (page > totalPages) page = totalPages;
        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageSizeOptions = PageSizeOptions;
        return View(paged);
    }

    private async Task<List<Category>> LoadParentsAsync(Guid? excludeId)
    {
        var all = await _db.Categories
            .Where(c => c.SiteId == CurrentSiteId || c.SiteId == null)
            .OrderBy(c => c.Type).ThenBy(c => c.Ord)
            .ToListAsync();
        if (excludeId.HasValue) all = all.Where(c => c.Id != excludeId.Value).ToList();
        return all;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Thêm danh mục";
        ViewBag.ParentCategories = await LoadParentsAsync(null);
        return View("Form", new Category { ActiveFlag = 1, Type = Constants.TypeMainMenu, Level = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category c)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ParentCategories = await LoadParentsAsync(null);
            return View("Form", c);
        }
        c.SiteId = CurrentSiteId;
        c.CreatedByUser = CurrentUser?.Id;
        if (c.ParentId.HasValue) c.Level = 2; else c.Level = 1;
        await _service.CreateAsync(c);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo danh mục", $"{u.UserName} tạo cat: {c.NameL} ({c.Type})");
        TempData["Success"] = "Đã thêm danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c == null) return NotFound();
        ViewBag.Title = "Sửa danh mục: " + c.NameL;
        ViewBag.ParentCategories = await LoadParentsAsync(id);
        return View("Form", c);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category c)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ParentCategories = await LoadParentsAsync(c.Id);
            return View("Form", c);
        }
        c.LuUserId = CurrentUser?.Id;
        if (c.ParentId.HasValue) c.Level = 2; else c.Level = 1;
        await _service.UpdateAsync(c);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa danh mục", $"{u.UserName} sửa cat: {c.NameL}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c == null) return NotFound();
        await _service.DeleteAsync(id);
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá danh mục", $"{u.UserName} xoá cat: {c.NameL}");
        TempData["Success"] = "Đã xoá (ẩn) danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        var oldVal = c.ActiveFlag;
        c.ActiveFlag = c.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle danh mục",
            $"{u.UserName} {(c.ActiveFlag == 1 ? "BẬT" : "TẮT")} cat '{c.NameL}' ({oldVal}→{c.ActiveFlag})");
        TempData["Success"] = c.ActiveFlag == 1 ? "Đã bật danh mục." : "Đã tắt danh mục.";
        return RedirectToAction(nameof(Index));
    }
}
