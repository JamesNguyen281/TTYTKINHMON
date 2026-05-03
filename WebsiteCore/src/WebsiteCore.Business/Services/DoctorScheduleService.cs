using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business.ViewModels;
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
    /// <summary>
    /// Tự sinh lịch trực hàng tháng cho mọi BS active của site. Idempotent — bỏ qua BS đã có
    /// schedule với ValidFrom = ngày 1 tháng đó. Mỗi BS được tạo Mon→Fri × {sáng, chiều} = 10 slot.
    /// </summary>
    Task<MonthlyScheduleResult> GenerateMonthlyScheduleAsync(int year, int month, Guid siteId, Guid? createdBy);

    /// <summary>
    /// Trả danh sách BS có lịch trực vào (date, session) thuộc site, kèm số slot còn lại.
    /// Sort: BS đang rảnh nhất (BookedSlots ASC) → giúp lễ tân phân bổ đều.
    /// Filter theo deptId nếu khác null (chỉ BS thuộc khoa đó).
    /// </summary>
    Task<List<DoctorAvailabilityVm>> GetAvailableDoctorsAsync(Guid siteId, Guid? deptId, DateOnly date, string session);

    /// <summary>
    /// Trả overview slot của một khoa tại (date, session): tổng quota khoa + chi tiết từng BS.
    /// </summary>
    Task<DepartmentSlotOverviewVm?> GetDepartmentSlotOverviewAsync(Guid siteId, Guid deptId, DateOnly date, string session);
}

public class MonthlyScheduleResult
{
    public int Created { get; set; }
    public int SkippedExisting { get; set; }
    public int DoctorsProcessed { get; set; }
    public List<string> SkippedDoctorNames { get; } = new();
}

public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly TtytlpDbContext _db;
    public DoctorScheduleService(TtytlpDbContext db) => _db = db;

    // Process-wide lock — chống race khi manual trigger + cron auto-gen chạy đồng thời cùng tháng.
    // Cùng process mới hiệu lực; production multi-instance cần thêm DB unique constraint.
    private static readonly SemaphoreSlim _autoGenLock = new(1, 1);

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

    public async Task<MonthlyScheduleResult> GenerateMonthlyScheduleAsync(int year, int month, Guid siteId, Guid? createdBy)
    {
        var result = new MonthlyScheduleResult();
        if (year < 2020 || year > 2100 || month < 1 || month > 12)
            return result;

        // Serialize toàn bộ auto-gen trong process — chống race manual trigger ↔ cron.
        await _autoGenLock.WaitAsync();
        try
        {
            return await GenerateMonthlyScheduleInternalAsync(year, month, siteId, createdBy);
        }
        finally
        {
            _autoGenLock.Release();
        }
    }

    private async Task<MonthlyScheduleResult> GenerateMonthlyScheduleInternalAsync(int year, int month, Guid siteId, Guid? createdBy)
    {
        var result = new MonthlyScheduleResult();
        var firstDay = new DateOnly(year, month, 1);
        var lastDay  = firstDay.AddMonths(1).AddDays(-1);

        // Lấy BS site hiện tại + có khoa active. Loại BS không khoa (Giám đốc) — họ không khám trực tiếp.
        var doctors = await (from d in _db.Doctors
                             join dep in _db.Departments on d.DepartmentId equals dep.Id
                             where d.ActiveFlag == 1 && dep.SiteId == siteId && dep.ActiveFlag == 1
                             orderby d.Ord
                             select d).ToListAsync();

        result.DoctorsProcessed = doctors.Count;
        if (doctors.Count == 0) return result;

        // Mặc định: thứ 2 → thứ 6 (weekday 2→6). Mỗi ngày 2 ca sáng+chiều.
        // Phòng để rỗng (admin tự bổ sung). MaxPatients = quota mặc định.
        byte[] weekdays = { 2, 3, 4, 5, 6 };
        string[] sessions = { Constants.SessionMorning, Constants.SessionAfternoon };

        var newSchedules = new List<DoctorSchedule>();
        foreach (var doc in doctors)
        {
            // Idempotency: BS đã có ít nhất 1 lịch active với ValidFrom = ngày 1 tháng đó → skip.
            // Tránh chạy 2 lần (manual + auto cron) tạo lịch trùng.
            var hasExisting = await _db.DoctorSchedules
                .AnyAsync(s => s.DoctorId == doc.Id
                            && s.ActiveFlag == 1
                            && s.ValidFrom == firstDay);
            if (hasExisting)
            {
                result.SkippedExisting++;
                if (!string.IsNullOrEmpty(doc.NameL)) result.SkippedDoctorNames.Add(doc.NameL);
                continue;
            }

            foreach (var wd in weekdays)
            foreach (var ss in sessions)
            {
                newSchedules.Add(new DoctorSchedule
                {
                    Id            = Guid.NewGuid(),
                    DoctorId      = doc.Id,
                    DepartmentId  = doc.DepartmentId,
                    Weekday       = wd,
                    Session       = ss,
                    Room          = null,
                    MaxPatients   = Constants.DefaultQuotaPerSession,
                    ValidFrom     = firstDay,
                    ValidTo       = lastDay,
                    Note          = $"Auto-gen tháng {month:00}/{year}",
                    ActiveFlag    = 1,
                    CreatedDate   = DateTime.Now,
                    CreatedBy     = createdBy
                });
            }
        }

        if (newSchedules.Count > 0)
        {
            _db.DoctorSchedules.AddRange(newSchedules);
            await _db.SaveChangesAsync();
        }
        result.Created = newSchedules.Count;
        return result;
    }

    /// <summary>
    /// Convert DateOnly → byte weekday theo quy ước DB: 1=CN, 2=T2..7=T7.
    /// (DateOnly.DayOfWeek: Sunday=0, Monday=1...Saturday=6).
    /// </summary>
    private static byte ToDbWeekday(DateOnly d) =>
        d.DayOfWeek == DayOfWeek.Sunday ? (byte)1 : (byte)((int)d.DayOfWeek + 1);

    public async Task<List<DoctorAvailabilityVm>> GetAvailableDoctorsAsync(
        Guid siteId, Guid? deptId, DateOnly date, string session)
    {
        if (string.IsNullOrEmpty(session)) return new();
        var weekday = ToDbWeekday(date);

        // Multi-site guard: chỉ join Doctor → Department → Site đúng site đang gọi.
        // Không trả về BS của site khác — kể cả khi caller truyền Doctor.Id thuộc site đó.
        var rows = await (from s in _db.DoctorSchedules
                          join d in _db.Doctors on s.DoctorId equals d.Id
                          join dep in _db.Departments on d.DepartmentId equals dep.Id
                          where s.ActiveFlag == 1
                             && s.Weekday == weekday
                             && s.Session == session
                             && s.ValidFrom <= date
                             && (s.ValidTo == null || s.ValidTo >= date)
                             && d.ActiveFlag == 1
                             && dep.SiteId == siteId
                             && dep.ActiveFlag == 1
                             && (deptId == null || d.DepartmentId == deptId)
                          select new
                          {
                              Schedule = s, Doctor = d, Dept = dep
                          }).AsNoTracking().ToListAsync();

        if (rows.Count == 0) return new();

        var doctorIds = rows.Select(r => r.Doctor.Id).ToList();
        var quotas = await _db.AppointmentQuota
            .Where(q => q.DoctorId != null
                     && doctorIds.Contains(q.DoctorId.Value)
                     && q.ApptDate == date
                     && q.Session == session)
            .AsNoTracking()
            .ToListAsync();

        // Đảm bảo phân bổ đều: sort theo (BookedSlots ASC, RemainingSlots DESC, Ord ASC)
        // → BS đang nhận ít BN nhất sẽ đứng đầu, lễ tân nhìn vào auto-pick là OK.
        var list = rows.Select(r =>
        {
            var q       = quotas.FirstOrDefault(qq => qq.DoctorId == r.Doctor.Id);
            var maxFromSched = r.Schedule.MaxPatients ?? Constants.DefaultQuotaPerSession;
            var max     = q?.MaxCount ?? maxFromSched;
            var booked  = q?.BookedCount ?? 0;
            return new DoctorAvailabilityVm
            {
                DoctorId       = r.Doctor.Id,
                DoctorName     = r.Doctor.NameL ?? r.Doctor.NameE ?? "",
                Position       = r.Doctor.Position,
                Specialty      = r.Doctor.SpeciallyL ?? r.Doctor.SpeciallyE,
                ImagePath      = r.Doctor.ImagePath,
                Ord            = r.Doctor.Ord ?? 0,
                DepartmentId   = r.Dept.Id,
                DepartmentName = r.Dept.NameL ?? r.Dept.NameE ?? "",
                Room           = r.Schedule.Room,
                Date           = date,
                Session        = session,
                MaxSlots       = max,
                BookedSlots    = booked,
            };
        })
        .OrderBy(x => x.BookedSlots)
        .ThenByDescending(x => x.MaxSlots)
        .ThenBy(x => x.Ord)
        .ToList();

        return list;
    }

    public async Task<DepartmentSlotOverviewVm?> GetDepartmentSlotOverviewAsync(
        Guid siteId, Guid deptId, DateOnly date, string session)
    {
        // Verify khoa đúng site — chống IDOR cross-site khi caller truyền deptId của site khác.
        var dept = await _db.Departments
            .FirstOrDefaultAsync(x => x.Id == deptId && x.SiteId == siteId && x.ActiveFlag == 1);
        if (dept == null) return null;

        // Quota tổng khoa (DoctorId == null = quota chung của khoa)
        var deptQuota = await _db.AppointmentQuota
            .FirstOrDefaultAsync(q => q.DepartmentId == deptId
                                   && q.DoctorId == null
                                   && q.ApptDate == date
                                   && q.Session == session);
        var deptMax    = deptQuota?.MaxCount    ?? Constants.DefaultQuotaPerSession;
        var deptBooked = deptQuota?.BookedCount ?? 0;

        var doctors = await GetAvailableDoctorsAsync(siteId, deptId, date, session);

        return new DepartmentSlotOverviewVm
        {
            DepartmentId    = dept.Id,
            DepartmentName  = dept.NameL ?? dept.NameE ?? "",
            Date            = date,
            Session         = session,
            DeptMaxSlots    = deptMax,
            DeptBookedSlots = deptBooked,
            Doctors         = doctors,
        };
    }
}
