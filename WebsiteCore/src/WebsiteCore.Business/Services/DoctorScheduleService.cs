using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IDoctorScheduleService
{
    Task<List<DoctorSchedule>> GetByDoctorAsync(Guid doctorId);
    Task<List<DoctorSchedule>> GetAllActiveAsync();
    Task<DoctorSchedule?> GetByIdAsync(Guid id);
    Task CreateAsync(DoctorSchedule s);
    Task UpdateAsync(DoctorSchedule s);
    Task DeleteAsync(Guid id);
}

public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly TtytlpDbContext _db;
    public DoctorScheduleService(TtytlpDbContext db) => _db = db;

    public Task<List<DoctorSchedule>> GetByDoctorAsync(Guid doctorId) =>
        _db.DoctorSchedules
           .Where(s => s.DoctorId == doctorId && s.ActiveFlag == 1)
           .OrderBy(s => s.Weekday).ThenBy(s => s.Session)
           .ToListAsync();

    public Task<List<DoctorSchedule>> GetAllActiveAsync() =>
        _db.DoctorSchedules
           .Where(s => s.ActiveFlag == 1)
           .OrderBy(s => s.Weekday).ThenBy(s => s.Session)
           .ToListAsync();

    public Task<DoctorSchedule?> GetByIdAsync(Guid id) =>
        _db.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id);

    public async Task CreateAsync(DoctorSchedule s)
    {
        if (s.Id == Guid.Empty) s.Id = Guid.NewGuid();
        if (s.CreatedDate == default) s.CreatedDate = DateTime.Now;
        if (s.ActiveFlag == 0) s.ActiveFlag = 1;
        _db.DoctorSchedules.Add(s);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(DoctorSchedule s)
    {
        var ex = await _db.DoctorSchedules.FirstOrDefaultAsync(x => x.Id == s.Id);
        if (ex == null) return;
        ex.DoctorId = s.DoctorId; ex.DepartmentId = s.DepartmentId;
        ex.Weekday = s.Weekday; ex.Session = s.Session; ex.Room = s.Room;
        ex.MaxPatients = s.MaxPatients;
        ex.ValidFrom = s.ValidFrom; ex.ValidTo = s.ValidTo;
        ex.Note = s.Note; ex.ActiveFlag = s.ActiveFlag;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var s = await _db.DoctorSchedules.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return;
        s.ActiveFlag = 0;
        await _db.SaveChangesAsync();
    }
}
