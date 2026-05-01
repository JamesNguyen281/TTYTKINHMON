using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business.Services;

namespace WebsiteCore.Web.Controllers;

public class DocumentClientController : BaseController
{
    private readonly IDocumentService _service;

    public DocumentClientController(ISiteService siteService, IDocumentService service) : base(siteService)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        ViewBag.QuerySearch = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        var all = await _service.GetActiveAsync(CurrentSiteId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim().ToLower();
            all = all.Where(d =>
                (d.DocumentName ?? "").ToLower().Contains(k) ||
                (d.DocumentCode ?? "").ToLower().Contains(k) ||
                (d.Description ?? "").ToLower().Contains(k)
            ).ToList();
        }
        ViewBag.Total = all.Count;
        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        ViewBag.Title = "Văn bản — Tài liệu";
        return View(paged);
    }
}
