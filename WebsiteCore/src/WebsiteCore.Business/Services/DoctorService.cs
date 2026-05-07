using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IDoctorService
{
    Task<List<Doctor>> GetForHomeAsync(Guid siteId, bool isPartner, int take = 8);
    Task<List<Doctor>> GetAllAsync(Guid siteId);
    Task<List<Doctor>> SearchAsync(Guid siteId, string? keyword, int page, int pageSize);
    Task<int> CountAsync(Guid siteId, string? keyword);
    Task<Doctor?> GetByIdAsync(Guid id);
    Task CreateAsync(Doctor d);
    Task UpdateAsync(Doctor d);
    Task DeleteAsync(Guid id);
}

public class DoctorService : IDoctorService
{
    private readonly TtytlpDbContext _db;
    public DoctorService(TtytlpDbContext db) => _db = db;

    /// <summary>
    /// Lấy danh sách bs cho trang /bac-si và homepage. LEFT JOIN với Department để
    /// Ban Giám đốc (DepartmentId = null) vẫn xuất hiện. Filter site qua dept.SiteId
    /// nếu có dept; bs không có dept (Giám đốc) chỉ qua khi không có dept thuộc site khác.
    /// </summary>
    public Task<List<Doctor>> GetForHomeAsync(Guid siteId, bool isPartner, int take = 8) =>
        (from d in _db.Doctors
         from dep in _db.Departments.Where(x => x.Id == d.DepartmentId).DefaultIfEmpty()
         where d.ActiveFlag == 1
            && d.IsPartner == isPartner
            && d.ShowOnHome == true
            && (dep == null || (dep.ActiveFlag == 1 && dep.SiteId == siteId))
         orderby d.Ord
         select d).Take(take).ToListAsync();

    public Task<List<Doctor>> GetAllAsync(Guid siteId) =>
        (from d in _db.Doctors
         from dep in _db.Departments.Where(x => x.Id == d.DepartmentId).DefaultIfEmpty()
         where d.ActiveFlag == 1
            && (dep == null || dep.SiteId == siteId)
         orderby d.Ord
         select d).ToListAsync();

    public Task<List<Doctor>> SearchAsync(Guid siteId, string? keyword, int page, int pageSize)
    {
        var q = from d in _db.Doctors
                join dep in _db.Departments on d.DepartmentId equals dep.Id
                where dep.SiteId == siteId
                select d;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.ToLower();
            q = q.Where(d => (d.NameL ?? "").ToLower().Contains(k));
        }
        return q.OrderBy(d => d.Ord)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
    }

    public Task<int> CountAsync(Guid siteId, string? keyword)
    {
        var q = from d in _db.Doctors
                join dep in _db.Departments on d.DepartmentId equals dep.Id
                where dep.SiteId == siteId
                select d;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.ToLower();
            q = q.Where(d => (d.NameL ?? "").ToLower().Contains(k));
        }
        return q.CountAsync();
    }

    public Task<Doctor?> GetByIdAsync(Guid id) => _db.Doctors.FirstOrDefaultAsync(d => d.Id == id);

    public async Task CreateAsync(Doctor d)
    {
        if (d.Id == Guid.Empty) d.Id = Guid.NewGuid();
        d.CreatedDate ??= DateTime.Now;
        d.ActiveFlag ??= 1;
        _db.Doctors.Add(d);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Doctor d)
    {
        // Reload-then-apply (chống mass-assignment) — KHÔNG ghi đè CreatedDate/Id
        var existing = await _db.Doctors.FirstOrDefaultAsync(x => x.Id == d.Id);
        if (existing == null) return;
        existing.NameL                = d.NameL;
        existing.NameE                = d.NameE;
        existing.SpeciallyL           = d.SpeciallyL;
        existing.SpeciallyE           = d.SpeciallyE;
        existing.LanguageSpoken       = d.LanguageSpoken;
        existing.QuantificationL      = d.QuantificationL;
        existing.QuantificationE      = d.QuantificationE;
        existing.ExperiencesL         = d.ExperiencesL;
        existing.ExperiencesE         = d.ExperiencesE;
        existing.SpeciallyInterestsL  = d.SpeciallyInterestsL;
        existing.SpeciallyInterestsE  = d.SpeciallyInterestsE;
        existing.DepartmentId         = d.DepartmentId;
        existing.ImagePath            = d.ImagePath;
        existing.Gender               = d.Gender;
        existing.TimetableL           = d.TimetableL;
        existing.TimetableE           = d.TimetableE;
        existing.Position             = d.Position;
        existing.IsPartner            = d.IsPartner;
        existing.Ord                  = d.Ord;
        existing.ActiveFlag           = d.ActiveFlag;
        existing.ShowOnHome           = d.ShowOnHome;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var d = await _db.Doctors.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return;
        _db.Doctors.Remove(d);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            _db.Entry(d).State = EntityState.Unchanged;
            d.ActiveFlag = 0;
            await _db.SaveChangesAsync();
        }
    }
}
