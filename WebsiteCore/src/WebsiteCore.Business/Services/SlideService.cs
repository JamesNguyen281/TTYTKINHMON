using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface ISlideService
{
    Task<List<Slide>> GetActiveAsync(Guid siteId);
    Task<List<Slide>> GetAllAsync(Guid siteId);
    Task<Slide?> GetByIdAsync(Guid id);
    Task CreateAsync(Slide s);
    Task UpdateAsync(Slide s);
    Task DeleteAsync(Guid id);
}

public class SlideService : ISlideService
{
    private readonly TtytlpDbContext _db;
    public SlideService(TtytlpDbContext db) => _db = db;

    public Task<List<Slide>> GetActiveAsync(Guid siteId) =>
        _db.Slides
           .Where(s => s.ActiveFlag == 1 && (s.SiteId == siteId || s.SiteId == null))
           .OrderBy(s => s.Ord)
           .ToListAsync();

    public Task<List<Slide>> GetAllAsync(Guid siteId) =>
        _db.Slides
           .Where(s => s.SiteId == siteId || s.SiteId == null)
           .OrderBy(s => s.Ord)
           .ToListAsync();

    public Task<Slide?> GetByIdAsync(Guid id) =>
        _db.Slides.FirstOrDefaultAsync(s => s.Id == id);

    public async Task CreateAsync(Slide s)
    {
        if (s.Id == Guid.Empty) s.Id = Guid.NewGuid();
        s.CreatedDate ??= DateTime.Now;
        s.ActiveFlag ??= 1;
        _db.Slides.Add(s);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Slide s)
    {
        var ex = await _db.Slides.FirstOrDefaultAsync(x => x.Id == s.Id);
        if (ex == null) return;
        ex.Type = s.Type; ex.TitleL = s.TitleL; ex.TitleE = s.TitleE;
        ex.DescriptionL = s.DescriptionL; ex.DescriptionE = s.DescriptionE;
        ex.ImagePath = s.ImagePath; ex.Icon = s.Icon; ex.Link = s.Link;
        ex.CssClass = s.CssClass; ex.Ord = s.Ord; ex.ActiveFlag = s.ActiveFlag;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var s = await _db.Slides.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return;
        _db.Slides.Remove(s);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            _db.Entry(s).State = EntityState.Unchanged;
            s.ActiveFlag = 0;
            await _db.SaveChangesAsync();
        }
    }
}
