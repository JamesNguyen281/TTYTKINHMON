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
public class CommentController : BaseController
{
    private readonly TtytlpDbContext _db;
    private readonly IAuditService _audit;

    public CommentController(ISiteService siteService, TtytlpDbContext db, IAuditService audit) : base(siteService)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 30)
    {
        ViewBag.Title = "Hộp thư góp ý";
        var query = _db.Comments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            var pattern = "%" + k + "%";
            query = query.Where(c => EF.Functions.Like(c.UserName ?? "", pattern)
                                  || EF.Functions.Like(c.Email ?? "", pattern)
                                  || EF.Functions.Like(c.Message ?? "", pattern));
        }
        var total = await query.CountAsync();
        var list  = await query.OrderByDescending(c => c.CreatedDate)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        ViewBag.Q        = q;
        ViewBag.Page     = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total    = total;
        ViewBag.Pages    = (int)Math.Ceiling((double)total / pageSize);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRead(Guid id)
    {
        var c = await _db.Comments.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        c.ActiveFlag = c.ActiveFlag == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Toggle góp ý",
            $"{u.UserName} đánh dấu góp ý {c.Id} ({c.UserName}) là {(c.ActiveFlag == 1 ? "ĐÃ ĐỌC" : "CHƯA ĐỌC")}");
        TempData["Success"] = c.ActiveFlag == 1 ? "Đã đánh dấu đã đọc." : "Đã đánh dấu chưa đọc.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _db.Comments.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        _db.Comments.Remove(c);
        await _db.SaveChangesAsync();
        var u = CurrentUser!;
        await _audit.LogAsync(u.Id, "Xoá góp ý", $"{u.UserName} xoá góp ý của {c.UserName} ({c.Id})");
        TempData["Success"] = "Đã xoá góp ý.";
        return RedirectToAction(nameof(Index));
    }
}
