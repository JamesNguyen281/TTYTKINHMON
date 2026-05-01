using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IQuotaService
{
    Task<List<AppointmentQuotum>> GetByDateRangeAsync(DateOnly from, DateOnly to);
    Task<AppointmentQuotum?> GetOrCreateAsync(Guid departmentId, DateOnly date, string session);
    Task SetMaxAsync(Guid departmentId, DateOnly date, string session, int max);
}

public class QuotaService : IQuotaService
{
    private readonly TtytlpDbContext _db;
    public QuotaService(TtytlpDbContext db) => _db = db;

    public Task<List<AppointmentQuotum>> GetByDateRangeAsync(DateOnly from, DateOnly to) =>
        _db.AppointmentQuota
           .Where(q => q.ApptDate >= from && q.ApptDate <= to)
           .OrderBy(q => q.ApptDate).ThenBy(q => q.Session)
           .ToListAsync();

    public async Task<AppointmentQuotum?> GetOrCreateAsync(Guid departmentId, DateOnly date, string session)
    {
        var q = await _db.AppointmentQuota
            .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.ApptDate == date && x.Session == session);
        if (q == null)
        {
            q = new AppointmentQuotum
            {
                Id = Guid.NewGuid(),
                DepartmentId = departmentId,
                ApptDate = date,
                Session = session,
                MaxCount = Constants.DefaultQuotaPerSession,
                BookedCount = 0,
                CreatedDate = DateTime.Now
            };
            _db.AppointmentQuota.Add(q);
            await _db.SaveChangesAsync();
        }
        return q;
    }

    public async Task SetMaxAsync(Guid departmentId, DateOnly date, string session, int max)
    {
        var q = await GetOrCreateAsync(departmentId, date, session);
        if (q == null) return;
        q.MaxCount = Math.Max(0, max);
        q.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
