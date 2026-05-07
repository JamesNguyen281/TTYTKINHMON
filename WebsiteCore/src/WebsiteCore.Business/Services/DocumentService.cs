using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IDocumentService
{
    Task<List<Document>> GetActiveAsync(Guid siteId);
    Task<List<Document>> GetAllAsync(Guid siteId);
    Task<Document?> GetByIdAsync(Guid id);
    Task CreateAsync(Document d);
    Task UpdateAsync(Document d);
    Task DeleteAsync(Guid id);
}

public class DocumentService : IDocumentService
{
    private readonly TtytlpDbContext _db;
    public DocumentService(TtytlpDbContext db) => _db = db;

    public Task<List<Document>> GetActiveAsync(Guid siteId) =>
        _db.Documents
           .Where(d => d.ActiveFlag == 1 && (d.SiteId == siteId || d.SiteId == null))
           .OrderByDescending(d => d.DocumentDate)
           .ToListAsync();

    public Task<List<Document>> GetAllAsync(Guid siteId) =>
        _db.Documents
           .Where(d => d.SiteId == siteId || d.SiteId == null)
           .OrderByDescending(d => d.CreatedDateTime)
           .ToListAsync();

    public Task<Document?> GetByIdAsync(Guid id) =>
        _db.Documents.FirstOrDefaultAsync(d => d.Id == id);

    public async Task CreateAsync(Document d)
    {
        if (d.Id == Guid.Empty) d.Id = Guid.NewGuid();
        d.CreatedDateTime ??= DateTime.Now;
        d.ActiveFlag ??= 1;
        _db.Documents.Add(d);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Document d)
    {
        var ex = await _db.Documents.FirstOrDefaultAsync(x => x.Id == d.Id);
        if (ex == null) return;
        ex.CategoryId = d.CategoryId; ex.Type = d.Type;
        ex.EffectiveFromDate = d.EffectiveFromDate; ex.EffectiveToDate = d.EffectiveToDate;
        ex.DocumentName = d.DocumentName; ex.DocumentCode = d.DocumentCode;
        ex.DocumentDate = d.DocumentDate; ex.AttachFilePath = d.AttachFilePath;
        ex.BinLocation = d.BinLocation; ex.Description = d.Description;
        ex.Owner = d.Owner; ex.ApprovedDate = d.ApprovedDate; ex.ApprovedBy = d.ApprovedBy;
        ex.ActiveFlag = d.ActiveFlag;
        ex.LuUserId = d.LuUserId; ex.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var d = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return;
        _db.Documents.Remove(d);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            _db.Entry(d).State = EntityState.Unchanged;
            d.ActiveFlag = 0;
            d.LuUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
    }
}
