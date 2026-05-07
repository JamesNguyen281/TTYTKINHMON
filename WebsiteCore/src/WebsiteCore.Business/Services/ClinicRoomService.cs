using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IClinicRoomService
{
    /// <summary>Tất cả ClinicRoom active của site (qua join Department.SiteId — multi-tenant guard).</summary>
    Task<List<ClinicRoom>> GetActiveBySiteAsync(Guid siteId);

    /// <summary>ClinicRoom theo Id, có verify cùng site (chống IDOR cross-site).</summary>
    Task<ClinicRoom?> GetByIdInSiteAsync(Guid id, Guid siteId);

    /// <summary>ClinicRoom thuộc 1 Department (chuyên khoa) cụ thể.</summary>
    Task<List<ClinicRoom>> GetByDepartmentAsync(Guid departmentId, Guid siteId);

    /// <summary>Số BS đang trực ở phòng vào ngày + ca cụ thể (qua DoctorSchedule.ClinicRoomId).</summary>
    Task<int> CountDoctorsOnDutyAsync(Guid roomId, DateOnly date, string session);

    /// <summary>Tổng appointment đã gán vào phòng cho ngày + ca (mọi status trừ cancelled/rejected).</summary>
    Task<int> CountAppointmentsAsync(Guid roomId, DateOnly date, string session);

    Task CreateAsync(ClinicRoom room);
    Task UpdateAsync(ClinicRoom room);
    Task<bool> DeleteAsync(Guid id, Guid siteId);
}

public class ClinicRoomService : IClinicRoomService
{
    private readonly TtytlpDbContext _db;
    public ClinicRoomService(TtytlpDbContext db) => _db = db;

    public Task<List<ClinicRoom>> GetActiveBySiteAsync(Guid siteId) =>
        (from cr in _db.ClinicRooms
         join dep in _db.Departments on cr.DepartmentId equals dep.Id
         where cr.ActiveFlag == 1 && dep.SiteId == siteId && dep.ActiveFlag == 1
         orderby cr.Ord, cr.RoomCode
         select cr).AsNoTracking().ToListAsync();

    public Task<ClinicRoom?> GetByIdInSiteAsync(Guid id, Guid siteId) =>
        (from cr in _db.ClinicRooms
         join dep in _db.Departments on cr.DepartmentId equals dep.Id
         where cr.Id == id && dep.SiteId == siteId
         select cr).FirstOrDefaultAsync();

    public Task<List<ClinicRoom>> GetByDepartmentAsync(Guid departmentId, Guid siteId) =>
        (from cr in _db.ClinicRooms
         join dep in _db.Departments on cr.DepartmentId equals dep.Id
         where cr.ActiveFlag == 1
            && cr.DepartmentId == departmentId
            && dep.SiteId == siteId
            && dep.ActiveFlag == 1
         orderby cr.Ord, cr.RoomCode
         select cr).AsNoTracking().ToListAsync();

    public Task<int> CountDoctorsOnDutyAsync(Guid roomId, DateOnly date, string session)
    {
        var weekday = date.DayOfWeek == DayOfWeek.Sunday
            ? (byte)1 : (byte)((int)date.DayOfWeek + 1);
        return _db.DoctorSchedules
            .Where(s => s.ClinicRoomId == roomId
                     && s.Weekday == weekday
                     && s.Session == session
                     && s.ActiveFlag == 1
                     && s.ScheduleType == Constants.ScheduleTypeClinic
                     && s.ValidFrom <= date
                     && (s.ValidTo == null || s.ValidTo >= date))
            .CountAsync();
    }

    public Task<int> CountAppointmentsAsync(Guid roomId, DateOnly date, string session) =>
        _db.Appointments
            .Where(a => a.ClinicRoomId == roomId
                     && a.AppointmentDate == date
                     && a.Session == session
                     && a.Status != Constants.ApptCancelled
                     && a.Status != Constants.ApptRejected)
            .CountAsync();

    public async Task CreateAsync(ClinicRoom room)
    {
        if (room.Id == Guid.Empty) room.Id = Guid.NewGuid();
        if (room.CreatedDate == default) room.CreatedDate = DateTime.Now;
        if (room.ActiveFlag == 0) room.ActiveFlag = 1;
        if (string.IsNullOrEmpty(room.RoomCode)) throw new ArgumentException("RoomCode bắt buộc.");
        if (string.IsNullOrEmpty(room.RoomName)) throw new ArgumentException("RoomName bắt buộc.");
        _db.ClinicRooms.Add(room);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClinicRoom room)
    {
        var ex = await _db.ClinicRooms.FirstOrDefaultAsync(x => x.Id == room.Id);
        if (ex == null) return;
        ex.DepartmentId   = room.DepartmentId;
        ex.RoomCode       = room.RoomCode;
        ex.RoomName       = room.RoomName;
        ex.SpecialtyL     = room.SpecialtyL;
        ex.SpecialtyE     = room.SpecialtyE;
        ex.Floor          = room.Floor;
        ex.CommonSymptoms = room.CommonSymptoms;
        ex.Ord            = room.Ord;
        ex.ActiveFlag     = room.ActiveFlag;
        ex.LuUpdated      = DateTime.Now;
        ex.LuUserId       = room.LuUserId;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid siteId)
    {
        var room = await GetByIdInSiteAsync(id, siteId);
        if (room == null) return false;
        var entity = await _db.ClinicRooms.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return false;
        // Hard delete neu khong co Appointment ref; neu co -> soft delete giu lich su
        _db.ClinicRooms.Remove(entity);
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _db.Entry(entity).State = EntityState.Unchanged;
            entity.ActiveFlag = 0;
            entity.LuUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
