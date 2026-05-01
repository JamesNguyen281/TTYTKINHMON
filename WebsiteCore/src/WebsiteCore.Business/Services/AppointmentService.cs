using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IAppointmentService
{
    Task<BookingResult> CreateAsync(BookingInputModel input, Guid? patientUserId, Guid siteId);
    Task<List<AppointmentRow>> GetByPatientAsync(Guid patientUserId);
    Task<List<AppointmentRow>> GetByStatusAsync(string status, Guid siteId);
    Task<AppointmentRow?> GetByIdAsync(Guid id);
    Task<AppointmentRow?> GetByBookingCodeAsync(string code);
    Task<List<AppointmentRow>> GetTodayConfirmedAsync(Guid siteId);
    Task<List<AppointmentRow>> GetByDoctorAsync(Guid doctorId, DateTime fromDate, DateTime toDate);
    Task<UpdateStatusResult> UpdateStatusAsync(Guid id, string newStatus, string? staffNote, Guid staffUserId);
    Task<bool> MarkCheckedInAsync(Guid id, Guid staffUserId);
    Task<int> CountUpdatesForPatientSinceAsync(Guid patientUserId, DateTime since);
    /// <summary>Phân/đổi bác sĩ phụ trách cho lịch hẹn. Set null để bỏ phân.
    /// Chỉ được gọi khi appt status ∈ {pending, confirmed, rescheduled} — không cho đổi khi đã completed/cancelled/rejected.</summary>
    Task<bool> AssignDoctorAsync(Guid apptId, Guid? doctorId, Guid staffUserId);
}

/// <summary>
/// Service xử lý nghiệp vụ đặt lịch khám.
///
/// Hai hành vi quan trọng:
///   - Khi pending → confirmed: sinh booking_code "KM yyMMdd S|C 6hex"
///   - Quota tự động giảm khi confirm, tăng khi cancel/reject (transactional)
///
/// Lưu ý: scaffolded entity dùng DateOnly cho cột date — phải convert từ DateTime.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly TtytlpDbContext _db;
    public AppointmentService(TtytlpDbContext db) => _db = db;

    public async Task<BookingResult> CreateAsync(BookingInputModel input, Guid? patientUserId, Guid siteId)
    {
        if (input == null)
            return new BookingResult { Success = false, ErrorMessage = "Dữ liệu không hợp lệ." };

        if (string.IsNullOrWhiteSpace(input.PatientName) || string.IsNullOrWhiteSpace(input.PatientPhone))
            return new BookingResult { Success = false, ErrorMessage = "Vui lòng nhập đầy đủ họ tên và SĐT." };

        if (!input.DepartmentId.HasValue)
            return new BookingResult { Success = false, ErrorMessage = "Vui lòng chọn chuyên khoa." };

        if (input.Session != Constants.SessionMorning && input.Session != Constants.SessionAfternoon)
            return new BookingResult { Success = false, ErrorMessage = "Buổi khám không hợp lệ." };

        var aDate = DateOnly.FromDateTime(input.AppointmentDate);
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (aDate < today)
            return new BookingResult { Success = false, ErrorMessage = "Ngày khám không được trong quá khứ." };
        if ((aDate.DayNumber - today.DayNumber) > Constants.MaxDaysAhead)
            return new BookingResult { Success = false, ErrorMessage = $"Ngày khám không vượt quá {Constants.MaxDaysAhead} ngày." };

        var dept = await _db.Departments.FindAsync(input.DepartmentId.Value);
        if (dept == null || dept.ActiveFlag != 1)
            return new BookingResult { Success = false, ErrorMessage = "Chuyên khoa không tồn tại hoặc đã ngừng tiếp nhận." };

        // Chống đặt lịch trùng cùng buổi cùng ngày — bệnh nhân đã login
        if (patientUserId.HasValue)
        {
            var dup = await _db.Appointments.AnyAsync(a =>
                a.PatientUserId == patientUserId &&
                a.DepartmentId  == input.DepartmentId &&
                a.AppointmentDate == aDate &&
                a.Session == input.Session &&
                (a.Status == Constants.ApptPending || a.Status == Constants.ApptConfirmed));
            if (dup)
                return new BookingResult { Success = false, ErrorMessage = "Bạn đã có lịch ở buổi này — vui lòng kiểm tra mục Lịch khám của tôi." };
        }

        var appt = new Appointment
        {
            Id              = Guid.NewGuid(),
            PatientUserId   = patientUserId,
            PatientName     = SafeTrim(input.PatientName, 150),
            PatientPhone    = SafeTrim(input.PatientPhone, 50),
            PatientEmail    = string.IsNullOrWhiteSpace(input.PatientEmail) ? null : SafeTrim(input.PatientEmail, 100),
            DepartmentId    = input.DepartmentId,
            DepartmentName  = dept.NameL,
            AppointmentDate = aDate,
            Session         = input.Session,
            Reason          = SafeTrim(input.Reason, 500),
            Status          = Constants.ApptPending,
            CheckedIn       = false,
            CreatedDate     = DateTime.Now,
            SiteId          = siteId
        };
        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        return new BookingResult { Success = true, AppointmentId = appt.Id };
    }

    private static string? SafeTrim(string? s, int maxLen)
    {
        if (s == null) return null;
        s = s.Trim();
        return s.Length > maxLen ? s.Substring(0, maxLen) : s;
    }

    public async Task<List<AppointmentRow>> GetByPatientAsync(Guid patientUserId)
    {
        var list = await _db.Appointments
            .Where(a => a.PatientUserId == patientUserId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.CreatedDate)
            .ToListAsync();
        return list.Select(MapRow).ToList();
    }

    public async Task<List<AppointmentRow>> GetByStatusAsync(string status, Guid siteId)
    {
        var list = await _db.Appointments
            .Where(a => a.Status == status && a.SiteId == siteId)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.Session).ThenBy(a => a.CreatedDate)
            .ToListAsync();
        return list.Select(MapRow).ToList();
    }

    public async Task<AppointmentRow?> GetByIdAsync(Guid id)
    {
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
        return a == null ? null : MapRow(a);
    }

    public async Task<AppointmentRow?> GetByBookingCodeAsync(string code)
    {
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.BookingCode == code);
        return a == null ? null : MapRow(a);
    }

    public async Task<List<AppointmentRow>> GetTodayConfirmedAsync(Guid siteId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var list = await _db.Appointments
            .Where(a => a.SiteId == siteId && a.Status == Constants.ApptConfirmed && a.AppointmentDate == today)
            .OrderBy(a => a.Session).ThenBy(a => a.BookingCode)
            .ToListAsync();
        return list.Select(MapRow).ToList();
    }

    public async Task<List<AppointmentRow>> GetByDoctorAsync(Guid doctorId, DateTime fromDate, DateTime toDate)
    {
        var f = DateOnly.FromDateTime(fromDate.Date);
        var t = DateOnly.FromDateTime(toDate.Date);
        var list = await _db.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= f
                     && a.AppointmentDate <= t
                     && (a.Status == Constants.ApptConfirmed || a.Status == Constants.ApptCompleted))
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.Session)
            .ToListAsync();
        return list.Select(MapRow).ToList();
    }

    /// <summary>
    /// Các transition hợp lệ — chống "leo trạng thái" tùy ý qua URL/form tampering.
    /// Map trả về: oldStatus → tập newStatus được phép.
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        [Constants.ApptPending]     = new() { Constants.ApptConfirmed, Constants.ApptRejected, Constants.ApptCancelled, Constants.ApptRescheduled },
        [Constants.ApptConfirmed]   = new() { Constants.ApptCompleted, Constants.ApptCancelled, Constants.ApptRescheduled },
        [Constants.ApptRescheduled] = new() { Constants.ApptConfirmed, Constants.ApptRejected,  Constants.ApptCancelled },
        // Trạng thái cuối — không cho đổi nữa
        [Constants.ApptRejected]    = new(),
        [Constants.ApptCancelled]   = new(),
        [Constants.ApptCompleted]   = new()
    };

    public async Task<UpdateStatusResult> UpdateStatusAsync(
        Guid id, string newStatus, string? staffNote, Guid staffUserId)
    {
        if (string.IsNullOrEmpty(newStatus))
            return UpdateStatusResult.Fail("Trạng thái mới không được để trống.");

        // Whitelist newStatus — không cho phép giá trị tự ý
        if (!AllowedTransitions.ContainsKey(newStatus))
            return UpdateStatusResult.Fail("Trạng thái không hợp lệ.");

        // Concurrency loop — retry tối đa 3 lần khi DbUpdateConcurrencyException
        for (int attempt = 0; attempt < 3; attempt++)
        {
            var appt = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
            if (appt == null) return UpdateStatusResult.Fail("Lịch hẹn không tồn tại.");

            var oldStatus = appt.Status ?? "";

            // Validate transition
            if (!AllowedTransitions.TryGetValue(oldStatus, out var allowed) || !allowed.Contains(newStatus))
                return UpdateStatusResult.Fail($"Không thể chuyển trạng thái '{oldStatus}' → '{newStatus}'.");

            // Reject phải có lý do (staffNote)
            if (newStatus == Constants.ApptRejected && string.IsNullOrWhiteSpace(staffNote))
                return UpdateStatusResult.Fail("Vui lòng nhập lý do từ chối.");

            var willConfirm  = newStatus == Constants.ApptConfirmed;
            var wasConfirmed = oldStatus == Constants.ApptConfirmed;

            // Quota delta: pending→confirmed = +1, confirmed→cancelled/rejected/rescheduled = -1
            int delta = 0;
            if (!wasConfirmed && willConfirm) delta = +1;
            else if (wasConfirmed && !willConfirm && newStatus != Constants.ApptCompleted) delta = -1;

            if (delta != 0 && appt.AppointmentDate.HasValue && !string.IsNullOrEmpty(appt.Session) && appt.DepartmentId.HasValue)
            {
                var deptId   = appt.DepartmentId.Value;
                var apptDate = appt.AppointmentDate.Value;
                var sess     = appt.Session;
                var quota = await _db.AppointmentQuota
                    .FirstOrDefaultAsync(q => q.DepartmentId == deptId
                                           && q.DoctorId == null
                                           && q.ApptDate == apptDate
                                           && q.Session == sess);
                if (quota == null)
                {
                    quota = new AppointmentQuotum
                    {
                        Id           = Guid.NewGuid(),
                        DepartmentId = deptId,
                        ApptDate     = apptDate,
                        Session      = sess,
                        MaxCount     = Constants.DefaultQuotaPerSession,
                        BookedCount  = Math.Max(0, delta),
                        CreatedDate  = DateTime.Now
                    };
                    _db.AppointmentQuota.Add(quota);
                }
                else
                {
                    // Khi confirm: từ chối nếu vượt quota max
                    if (delta > 0 && quota.BookedCount + delta > quota.MaxCount)
                        return UpdateStatusResult.Fail($"Buổi này đã hết suất ({quota.BookedCount}/{quota.MaxCount}).");
                    quota.BookedCount = quota.BookedCount + delta;
                    if (quota.BookedCount < 0) quota.BookedCount = 0;
                    quota.LuUpdated = DateTime.Now;
                }
            }

            // Sinh booking_code khi chuyển sang confirmed
            if (willConfirm && string.IsNullOrEmpty(appt.BookingCode))
            {
                var dateStr = appt.AppointmentDate.HasValue
                    ? appt.AppointmentDate.Value.ToDateTime(TimeOnly.MinValue).ToString("yyMMdd")
                    : DateTime.Today.ToString("yyMMdd");
                var sessTag = appt.Session == Constants.SessionMorning ? "S" : "C";
                var rand = Guid.NewGuid().ToString("N")[..6].ToUpper();
                appt.BookingCode = $"KM{dateStr}{sessTag}{rand}";
            }

            appt.Status    = newStatus;
            appt.StaffNote = SafeTrim(staffNote, 500);
            appt.LuUpdated = DateTime.Now;
            appt.LuUserId  = staffUserId;

            try
            {
                await _db.SaveChangesAsync();
                return new UpdateStatusResult
                {
                    Success     = true,
                    OldStatus   = oldStatus,
                    NewStatus   = newStatus,
                    BookingCode = appt.BookingCode
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                // Reload from DB và retry — race condition khi 2 nhân viên cùng lúc duyệt
                foreach (var entry in _db.ChangeTracker.Entries().ToList())
                    entry.State = EntityState.Detached;
                if (attempt == 2)
                    return UpdateStatusResult.Fail("Có người khác vừa cập nhật cùng lịch — vui lòng thử lại.");
            }
        }
        return UpdateStatusResult.Fail("Không thể cập nhật trạng thái.");
    }

    public Task<int> CountUpdatesForPatientSinceAsync(Guid patientUserId, DateTime since) =>
        _db.Appointments
           .Where(a => a.PatientUserId == patientUserId && a.LuUpdated.HasValue && a.LuUpdated >= since)
           .CountAsync();

    public async Task<bool> AssignDoctorAsync(Guid apptId, Guid? doctorId, Guid staffUserId)
    {
        var appt = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == apptId);
        if (appt == null) return false;
        // Không cho đổi nếu đã ở trạng thái cuối
        var status = appt.Status ?? "";
        if (status == Constants.ApptCompleted || status == Constants.ApptCancelled || status == Constants.ApptRejected)
            return false;
        appt.DoctorId  = doctorId;
        appt.LuUpdated = DateTime.Now;
        appt.LuUserId  = staffUserId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkCheckedInAsync(Guid id, Guid staffUserId)
    {
        var appt = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
        if (appt == null) return false;
        if (appt.Status != Constants.ApptConfirmed) return false;
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (appt.AppointmentDate != today) return false;
        if (appt.CheckedIn) return true;
        appt.CheckedIn = true;
        appt.LuUpdated = DateTime.Now;
        appt.LuUserId  = staffUserId;
        await _db.SaveChangesAsync();
        return true;
    }

    private static AppointmentRow MapRow(Appointment a) => new()
    {
        Id              = a.Id,
        SiteId          = a.SiteId,
        BookingCode     = a.BookingCode,
        PatientName     = a.PatientName,
        PatientPhone    = a.PatientPhone,
        PatientEmail    = a.PatientEmail,
        PatientUserId   = a.PatientUserId,
        DepartmentId    = a.DepartmentId,
        DepartmentName  = a.DepartmentName,
        DoctorId        = a.DoctorId,
        AppointmentDate = a.AppointmentDate?.ToDateTime(TimeOnly.MinValue),
        Session         = a.Session,
        Reason          = a.Reason,
        Status          = a.Status,
        StaffNote       = a.StaffNote,
        CheckedIn       = a.CheckedIn,
        CreatedDate     = a.CreatedDate
    };
}
