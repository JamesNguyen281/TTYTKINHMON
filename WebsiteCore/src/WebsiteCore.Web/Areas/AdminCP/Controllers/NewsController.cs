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
[StaffAuthorize(Constants.AdminGroup, Constants.PosterGroup)]
public class NewsController : BaseController
{
    private readonly TtytlpDbContext _db;
    private readonly ICategoryService _categoryService;
    private readonly IDepartmentService _deptService;
    private readonly IAuditService _audit;
    private readonly IWebHostEnvironment _env;

    public NewsController(
        ISiteService siteService,
        TtytlpDbContext db,
        ICategoryService categoryService,
        IDepartmentService deptService,
        IAuditService audit,
        IWebHostEnvironment env) : base(siteService)
    {
        _db = db;
        _categoryService = categoryService;
        _deptService = deptService;
        _audit = audit;
        _env = env;
    }

    public async Task<IActionResult> Index(string? q, string? type = "complete", bool mine = false, int take = 100)
    {
        var (filterFlag, title) = type switch
        {
            "waiting" => (0,  "Tin chờ duyệt"),
            "no"      => (-1, "Tin không duyệt"),
            "delete"  => (-2, "Tin đã xoá"),
            _         => (1,  "Tin đã đăng")
        };
        ViewBag.Title = mine ? "Bài của tôi" : title;
        ViewBag.Type = type ?? "complete";

        // Site scoping — POSTER/admin chỉ thấy News của site mình (hoặc null = global cross-site)
        var siteId = CurrentSiteId;
        var query = _db.News.Where(n => n.SiteId == siteId || n.SiteId == null);
        if (mine && CurrentUser != null)
        {
            var uid = CurrentUser.Id;
            query = query.Where(n => n.CreatedByUserId == uid);
        }
        else
        {
            query = query.Where(n => n.ActiveFlag == filterFlag);
        }
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(n => n.TitleL!.Contains(q));
        var list = await query.OrderByDescending(n => n.CreatedDate).Take(take).ToListAsync();
        ViewBag.Q = q;
        ViewBag.Mine = mine;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Thêm tin tức";
        ViewBag.Categories = await _categoryService.GetAllAsync(CurrentSiteId);
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View("Form", new News { ActiveFlag = 1, ShowOnHome = false, HotNew = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(News n, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllAsync(CurrentSiteId);
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View("Form", n);
        }
        var saved = await FileUploadHelper.SaveImageAsync(imageFile, _env, "news");
        if (!string.IsNullOrEmpty(saved)) n.ImagePath = saved;
        n.Id = Guid.NewGuid();
        n.SiteId = CurrentSiteId;
        n.CreatedByUserId = CurrentUser?.Id;
        n.CreatedDate = DateTime.Now;
        n.ActiveFlag ??= 1;
        // H1: sanitize HTML chống XSS stored (POSTER editable detail)
        n.DetailL = WebsiteCore.Business.Helpers.StringHelper.SanitizeHtml(n.DetailL);
        if (string.IsNullOrEmpty(n.AliasL) && !string.IsNullOrEmpty(n.TitleL))
            n.AliasL = WebsiteCore.Business.Helpers.StringHelper.ChangeText(n.TitleL);
        n.Link = "tin-tuc/" + n.AliasL;
        _db.News.Add(n);
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Tạo tin", $"{u.UserName} tạo: {n.TitleL}");
        TempData["Success"] = "Đã thêm tin.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        ViewBag.Title = "Sửa tin: " + n.TitleL;
        ViewBag.Categories = await _categoryService.GetAllAsync(CurrentSiteId);
        ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        return View("Form", n);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(News n, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllAsync(CurrentSiteId);
            ViewBag.Departments = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
            return View("Form", n);
        }
        // Reload-then-apply (chống mass-assignment): không trust SiteId/Id/CreatedDate/CreatedByUserId từ form
        var existing = await _db.News.FirstOrDefaultAsync(x => x.Id == n.Id);
        if (existing == null) return NotFound();
        // Site scoping — POSTER site A không sửa được News site B qua tampering Id
        if (existing.SiteId != CurrentSiteId && existing.SiteId != null) return NotFound();

        var saved = await FileUploadHelper.SaveImageAsync(imageFile, _env, "news");
        if (!string.IsNullOrEmpty(saved)) existing.ImagePath = saved;
        else if (!string.IsNullOrEmpty(n.ImagePath)) existing.ImagePath = n.ImagePath;

        existing.TitleL          = n.TitleL;
        existing.TitleE          = n.TitleE;
        existing.AliasL          = n.AliasL;
        existing.AliasE          = n.AliasE;
        // H1: sanitize HTML rich content (POSTER có quyền edit — chống XSS stored)
        existing.DetailL         = WebsiteCore.Business.Helpers.StringHelper.SanitizeHtml(n.DetailL);
        existing.DetailE         = n.DetailE;
        existing.DescriptionL    = n.DescriptionL;
        existing.DescriptionE    = n.DescriptionE;
        existing.CategoryId      = n.CategoryId;
        existing.DepartmentId    = n.DepartmentId;
        existing.MetaDescription = n.MetaDescription;
        existing.MetaKeyword     = n.MetaKeyword;
        existing.HotNew          = n.HotNew;
        existing.ShowOnHome      = n.ShowOnHome;
        existing.Type            = n.Type;
        existing.Ord             = n.Ord;
        existing.ActiveFlag      = n.ActiveFlag;
        existing.LuUserId        = CurrentUser?.Id;
        existing.LuUpdated       = DateTime.Now;
        if (!string.IsNullOrEmpty(existing.AliasL))
            existing.Link = "tin-tuc/" + existing.AliasL;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Sửa tin", $"{u.UserName} sửa: {existing.TitleL}");
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, string? returnType)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        n.ActiveFlag = -2;
        n.LuUserId = CurrentUser?.Id;
        n.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá tin", $"{u.UserName} xoá: {n.TitleL}");
        TempData["Success"] = "Đã chuyển vào thùng rác.";
        return RedirectToAction(nameof(Index), new { type = returnType ?? "complete" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, string? returnType)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        n.ActiveFlag = 1;
        n.LuUserId = CurrentUser?.Id;
        n.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Duyệt tin", $"{u.UserName} duyệt: {n.TitleL}");
        TempData["Success"] = "Đã duyệt và đăng tin.";
        return RedirectToAction(nameof(Index), new { type = returnType ?? "waiting" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string? returnType)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        n.ActiveFlag = -1;
        n.LuUserId = CurrentUser?.Id;
        n.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Từ chối tin", $"{u.UserName} từ chối: {n.TitleL}");
        TempData["Success"] = "Đã chuyển sang Tin không duyệt.";
        return RedirectToAction(nameof(Index), new { type = returnType ?? "waiting" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(Guid id, string? returnType)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        n.ActiveFlag = 0; // chuyển về chờ duyệt để admin review
        n.LuUserId = CurrentUser?.Id;
        n.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Khôi phục tin", $"{u.UserName} khôi phục: {n.TitleL}");
        TempData["Success"] = "Đã khôi phục — chuyển về Chờ duyệt.";
        return RedirectToAction(nameof(Index), new { type = returnType ?? "delete" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleShowHome(Guid id, string? returnType)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        n.ShowOnHome = !(n.ShowOnHome ?? false);
        n.LuUserId = CurrentUser?.Id;
        n.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle hiển thị tin",
            $"{u.UserName} {(n.ShowOnHome == true ? "BẬT" : "TẮT")} ShowOnHome: {n.TitleL}");
        TempData["Success"] = n.ShowOnHome == true ? "Đã bật hiển thị trang chủ." : "Đã ẩn khỏi trang chủ.";
        return RedirectToAction(nameof(Index), new { type = returnType ?? "complete" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleHot(Guid id, string? returnType)
    {
        var n = await _db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        n.HotNew = !(n.HotNew ?? false);
        n.LuUserId = CurrentUser?.Id;
        n.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle hot tin",
            $"{u.UserName} {(n.HotNew == true ? "BẬT" : "TẮT")} HotNew: {n.TitleL}");
        TempData["Success"] = n.HotNew == true ? "Đã đánh dấu Hot." : "Đã bỏ đánh dấu Hot.";
        return RedirectToAction(nameof(Index), new { type = returnType ?? "complete" });
    }
}
