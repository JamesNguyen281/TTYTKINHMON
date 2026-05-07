using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IVideoService
{
    Task<List<Video>> GetForHomeAsync(Guid siteId, int take = 6);
    Task<List<Video>> GetAllAsync(Guid siteId);
    Task<Video?> GetByIdAsync(Guid id);
    Task CreateAsync(Video v);
    Task UpdateAsync(Video v);
    Task DeleteAsync(Guid id);
}

public class VideoService : IVideoService
{
    private readonly TtytlpDbContext _db;
    public VideoService(TtytlpDbContext db) => _db = db;

    public Task<List<Video>> GetForHomeAsync(Guid siteId, int take = 6) =>
        _db.Videos
           .Where(v => (v.SiteId == siteId || v.SiteId == null)
                    && v.Status == 1)
           .OrderBy(v => v.Ord)
           .Take(take)
           .ToListAsync();

    public Task<List<Video>> GetAllAsync(Guid siteId) =>
        _db.Videos
           .Where(v => v.SiteId == siteId || v.SiteId == null)
           .OrderBy(v => v.Ord)
           .ToListAsync();

    public Task<Video?> GetByIdAsync(Guid id) =>
        _db.Videos.FirstOrDefaultAsync(v => v.VideoId == id);

    public async Task CreateAsync(Video v)
    {
        if (v.VideoId == Guid.Empty) v.VideoId = Guid.NewGuid();
        v.CreatedDate ??= DateTime.Now;
        v.Status ??= 1;
        _db.Videos.Add(v);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Video v)
    {
        var ex = await _db.Videos.FirstOrDefaultAsync(x => x.VideoId == v.VideoId);
        if (ex == null) return;
        ex.VideoTitleL = v.VideoTitleL; ex.VideoTitleE = v.VideoTitleE;
        ex.VideoDescriptionL = v.VideoDescriptionL; ex.VideoDescriptionE = v.VideoDescriptionE;
        ex.VideoThumbnail = v.VideoThumbnail; ex.VideoLink = v.VideoLink;
        ex.Ord = v.Ord; ex.Status = v.Status;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var v = await _db.Videos.FirstOrDefaultAsync(x => x.VideoId == id);
        if (v == null) return;
        _db.Videos.Remove(v);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            _db.Entry(v).State = EntityState.Unchanged;
            v.Status = 0;
            await _db.SaveChangesAsync();
        }
    }
}
