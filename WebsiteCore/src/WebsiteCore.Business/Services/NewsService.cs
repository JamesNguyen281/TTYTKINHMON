using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface INewsService
{
    Task<List<News>> GetTopAsync(Guid siteId, int take = 8);
    Task<List<News>> GetByCategoryAliasAsync(Guid siteId, string alias, int page, int pageSize);
    Task<News?> GetByAliasAsync(string alias);
    Task<List<Category>> GetMenuAsync(Guid siteId);
    Task<List<Category>> GetOutstandingServicesAsync(Guid siteId, int take = 6);
}

public class NewsService : INewsService
{
    private readonly TtytlpDbContext _db;
    public NewsService(TtytlpDbContext db) => _db = db;

    public Task<List<News>> GetTopAsync(Guid siteId, int take = 8) =>
        _db.News
           .Where(n => (n.SiteId == siteId || n.SiteId == null)
                    && n.ActiveFlag == 1
                    && n.HotNew == true)
           .OrderByDescending(n => n.CreatedDate)
           .Take(take)
           .ToListAsync();

    public async Task<List<News>> GetByCategoryAliasAsync(Guid siteId, string alias, int page, int pageSize)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.AliasL == alias);
        if (cat == null) return new List<News>();
        return await _db.News
            .Where(n => n.CategoryId == cat.Id
                     && n.ActiveFlag == 1
                     && (n.SiteId == siteId || n.SiteId == null))
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<News?> GetByAliasAsync(string alias) =>
        _db.News.FirstOrDefaultAsync(n => n.AliasL == alias && n.ActiveFlag == 1);

    public Task<List<Category>> GetMenuAsync(Guid siteId) =>
        _db.Categories
            .Where(c => c.Type == Constants.TypeMainMenu
                     && c.ActiveFlag == 1
                     && (c.SiteId == siteId || c.SiteId == null)
                     && c.ParentId == null)
            .OrderBy(c => c.Ord)
            .ToListAsync();

    public Task<List<Category>> GetOutstandingServicesAsync(Guid siteId, int take = 6) =>
        _db.Categories
            .Where(c => c.Type == Constants.TypeOutstandingService
                     && c.ActiveFlag == 1
                     && (c.SiteId == siteId || c.SiteId == null))
            .OrderBy(c => c.Ord)
            .Take(take)
            .ToListAsync();
}
