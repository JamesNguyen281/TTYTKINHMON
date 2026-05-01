using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business.Services;

namespace WebsiteCore.Web.Controllers;

/// <summary>
/// Sitemap.xml + robots.txt cho SEO. Dynamic: liệt kê /, /bac-si, /dat-lich-kham,
/// menu chính, departments, news 200 mục mới nhất.
/// </summary>
public class SeoController : BaseController
{
    private readonly INewsService _newsService;
    private readonly ICategoryService _categoryService;
    private readonly IDepartmentService _departmentService;

    public SeoController(
        ISiteService siteService,
        INewsService newsService,
        ICategoryService categoryService,
        IDepartmentService departmentService) : base(siteService)
    {
        _newsService = newsService;
        _categoryService = categoryService;
        _departmentService = departmentService;
    }

    [Route("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var entries = new List<SitemapEntry>
        {
            new(baseUrl + "/",              now, "daily",   "1.0"),
            new(baseUrl + "/bac-si",        now, "weekly",  "0.8"),
            new(baseUrl + "/dat-lich-kham", now, "monthly", "0.9"),
            new(baseUrl + "/dat-cau-hoi",   now, "monthly", "0.7"),
            new(baseUrl + "/hoi-dap",       now, "weekly",  "0.7"),
            new(baseUrl + "/van-ban",       now, "monthly", "0.6"),
        };

        try
        {
            var menus = await _categoryService.GetMainMenuAsync(CurrentSiteId, 20);
            foreach (var m in menus)
            {
                if (!string.IsNullOrEmpty(m.AliasL))
                    entries.Add(new(baseUrl + "/chuyen-muc/" + m.AliasL, now, "weekly", "0.7"));
            }
        }
        catch { }

        try
        {
            var depts = await _departmentService.GetActiveBySiteAsync(CurrentSiteId);
            foreach (var d in depts)
            {
                if (!string.IsNullOrEmpty(d.Alias))
                    entries.Add(new(baseUrl + "/chuyen-khoa/" + d.Alias, now, "weekly", "0.7"));
            }
        }
        catch { }

        try
        {
            var news = await _newsService.GetTopAsync(CurrentSiteId, 200);
            foreach (var n in news)
            {
                if (!string.IsNullOrEmpty(n.AliasL))
                {
                    var lastMod = n.CreatedDate?.ToString("yyyy-MM-dd") ?? now;
                    entries.Add(new(baseUrl + "/tin-tuc/" + n.AliasL, lastMod, "monthly", "0.6"));
                }
            }
        }
        catch { }

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var e in entries)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine("    <loc>" + System.Security.SecurityElement.Escape(e.Loc) + "</loc>");
            sb.AppendLine("    <lastmod>" + e.LastMod + "</lastmod>");
            sb.AppendLine("    <changefreq>" + e.ChangeFreq + "</changefreq>");
            sb.AppendLine("    <priority>" + e.Priority + "</priority>");
            sb.AppendLine("  </url>");
        }
        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    [Route("robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /AdminCP/");
        sb.AppendLine("Disallow: /le-tan");
        sb.AppendLine("Disallow: /bac-si-portal");
        sb.AppendLine("Disallow: /ho-so");
        sb.AppendLine();
        sb.AppendLine("Sitemap: " + baseUrl + "/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    private record SitemapEntry(string Loc, string LastMod, string ChangeFreq, string Priority);
}
