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
public class AuditSystemsController : BaseController
{
    private readonly TtytlpDbContext _db;

    public AuditSystemsController(ISiteService siteService, TtytlpDbContext db) : base(siteService) => _db = db;

    public async Task<IActionResult> Index(string? q, int take = 100)
    {
        ViewBag.Title = "Nhật ký hệ thống";
        var query = _db.AuditSystems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.ActionDescription!.Contains(q) || a.ActionDetail!.Contains(q));
        var rows = await query.OrderByDescending(a => a.ActionDate).Take(take).ToListAsync();
        ViewBag.Q = q;
        ViewBag.Take = take;
        return View(rows);
    }
}
