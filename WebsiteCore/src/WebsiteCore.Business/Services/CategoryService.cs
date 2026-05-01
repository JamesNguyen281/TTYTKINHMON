using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface ICategoryService
{
    Task<List<Category>> GetSlideBoxAsync(Guid siteId, int take = 3);
    Task<List<Category>> GetSlideTextBoxAsync(Guid siteId, int take = 6);
    Task<List<Category>> GetByTypeAsync(Guid siteId, string type);
    Task<List<Category>> GetMainMenuAsync(Guid siteId, int take = 12);
    Task<List<Category>> GetChildrenAsync(Guid parentId);
    Task<List<Category>> GetAllAsync(Guid siteId);
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetByAliasAsync(string alias);
    Task CreateAsync(Category c);
    Task UpdateAsync(Category c);
    Task DeleteAsync(Guid id);
}

public class CategoryService : ICategoryService
{
    private readonly TtytlpDbContext _db;
    public CategoryService(TtytlpDbContext db) => _db = db;

    public Task<List<Category>> GetSlideBoxAsync(Guid siteId, int take = 3) =>
        _db.Categories
           .Where(c => c.Type == Constants.TypeBlockSlide
                    && c.ActiveFlag == 1
                    && (c.SiteId == siteId || c.SiteId == null))
           .OrderBy(c => c.Ord)
           .Take(take)
           .ToListAsync();

    public Task<List<Category>> GetSlideTextBoxAsync(Guid siteId, int take = 6) =>
        _db.Categories
           .Where(c => c.Type == Constants.TypeBlockSlideText
                    && c.ActiveFlag == 1
                    && (c.SiteId == siteId || c.SiteId == null))
           .OrderBy(c => c.Ord)
           .Take(take)
           .ToListAsync();

    public Task<List<Category>> GetByTypeAsync(Guid siteId, string type) =>
        _db.Categories
           .Where(c => c.Type == type
                    && c.ActiveFlag == 1
                    && (c.SiteId == siteId || c.SiteId == null))
           .OrderBy(c => c.Ord)
           .ToListAsync();

    public Task<List<Category>> GetMainMenuAsync(Guid siteId, int take = 8) =>
        _db.Categories
           .Where(c => (c.Type == Constants.TypeMainMenu || c.Type == "THEME_LIST_IMAGES")
                    && c.ActiveFlag == 1
                    && c.Level == 1
                    && c.ParentId == null
                    && c.ShowOnHome == true
                    && (c.SiteId == siteId || c.SiteId == null))
           .OrderBy(c => c.Ord)
           .Take(take)
           .ToListAsync();

    public Task<List<Category>> GetChildrenAsync(Guid parentId) =>
        _db.Categories
           .Where(c => c.ParentId == parentId && c.ActiveFlag == 1)
           .OrderBy(c => c.Ord)
           .ToListAsync();

    public Task<List<Category>> GetAllAsync(Guid siteId) =>
        _db.Categories
           .Where(c => c.SiteId == siteId || c.SiteId == null)
           .OrderBy(c => c.Type).ThenBy(c => c.Ord)
           .ToListAsync();

    public Task<Category?> GetByIdAsync(Guid id) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Category?> GetByAliasAsync(string alias) =>
        _db.Categories.FirstOrDefaultAsync(c => c.AliasL == alias && c.ActiveFlag == 1);

    public async Task CreateAsync(Category c)
    {
        if (c.Id == Guid.Empty) c.Id = Guid.NewGuid();
        c.CreatedDate ??= DateTime.Now;
        c.ActiveFlag ??= 1;
        _db.Categories.Add(c);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category c)
    {
        // Reload-then-apply: chỉ trust field user được phép sửa, KHÔNG ghi đè
        // Id/SiteId/CreatedDate/CreatedByUser từ form input (chống mass-assignment).
        var existing = await _db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id);
        if (existing == null) return;
        existing.NameL          = c.NameL;
        existing.NameE          = c.NameE;
        existing.AliasL         = c.AliasL;
        existing.AliasE         = c.AliasE;
        existing.DescriptionL   = c.DescriptionL;
        existing.DescriptionE   = c.DescriptionE;
        existing.Type           = c.Type;
        existing.ParentId       = c.ParentId;
        existing.Level          = c.Level;
        existing.ImagePath      = c.ImagePath;
        existing.Link           = c.Link;
        existing.Ord            = c.Ord;
        existing.ActiveFlag     = c.ActiveFlag;
        existing.ShowOnHome     = c.ShowOnHome;
        existing.LuUpdated      = DateTime.Now;
        existing.LuUserId       = c.LuUserId;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return;
        c.ActiveFlag = 0;
        await _db.SaveChangesAsync();
    }
}
