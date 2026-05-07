using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IPartnerService
{
    Task<List<Partner>> GetActiveAsync(Guid siteId);
    Task<List<Partner>> GetAllAsync(Guid siteId);
    Task<Partner?> GetByIdAsync(Guid id);
    Task CreateAsync(Partner p);
    Task UpdateAsync(Partner p);
    Task DeleteAsync(Guid id);
}

public class PartnerService : IPartnerService
{
    private readonly TtytlpDbContext _db;
    public PartnerService(TtytlpDbContext db) => _db = db;

    public Task<List<Partner>> GetActiveAsync(Guid siteId) =>
        _db.Partners
           .Where(p => p.ActiveFlag == 1 && (p.SiteId == siteId || p.SiteId == null))
           .OrderBy(p => p.Ord)
           .ToListAsync();

    public Task<List<Partner>> GetAllAsync(Guid siteId) =>
        _db.Partners
           .Where(p => p.SiteId == siteId || p.SiteId == null)
           .OrderBy(p => p.Ord)
           .ToListAsync();

    public Task<Partner?> GetByIdAsync(Guid id) =>
        _db.Partners.FirstOrDefaultAsync(p => p.Id == id);

    public async Task CreateAsync(Partner p)
    {
        if (p.Id == Guid.Empty) p.Id = Guid.NewGuid();
        p.CreatedDate ??= DateTime.Now;
        p.ActiveFlag ??= 1;
        _db.Partners.Add(p);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Partner p)
    {
        var ex = await _db.Partners.FirstOrDefaultAsync(x => x.Id == p.Id);
        if (ex == null) return;
        ex.NameL = p.NameL; ex.NameE = p.NameE; ex.Link = p.Link;
        ex.ImagePath = p.ImagePath; ex.Ord = p.Ord; ex.ActiveFlag = p.ActiveFlag;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var p = await _db.Partners.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return;
        _db.Partners.Remove(p);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            _db.Entry(p).State = EntityState.Unchanged;
            p.ActiveFlag = 0;
            await _db.SaveChangesAsync();
        }
    }
}
