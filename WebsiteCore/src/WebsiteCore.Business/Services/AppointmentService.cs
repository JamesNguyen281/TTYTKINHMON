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
    /// <summary>Tra cứu mọi lịch theo SĐT bệnh nhân — dùng cho lễ tân giúp khách vãng lai (không có account).</summary>
    Task<List<AppointmentRow>> GetByPhoneAsync(string phone, Guid siteId);
    /// <summary>Lấy mọi lịch của một ngày bất kỳ — bao gồm tất cả status, dùng cho dashboard "lịch theo ngày".</summary>
    Task<List<AppointmentRow>> GetByDateAsync(DateOnly date, Guid siteId);
    Task<UpdateStatusResult> UpdateStatusAsync(Guid id, string newStatus, string? staffNote, Guid staffUserId);
    Task<bool> MarkCheckedInAsync(Guid id, Guid staffUserId);
    Task<int> CountUpdatesForPatientSinceAsync(Guid patientUserId, DateTime since);
    /// <summary>Phân/đổi bác sĩ phụ trách cho lịch hẹn. Set null để bỏ phân.
    /// Chỉ được gọi khi appt status ∈ {pending, confirmed, rescheduled} — không cho đổi khi đã completed/cancelled/rejected.</summary>
    Task<bool> AssignDoctorAsync(Guid apptId, Guid? doctorId, Guid staffUserId);

    /// <summary>
    /// P3.B — BS bấm "Hẹn khám lại" sau khi khám xong: sinh appointment con với
    /// status=confirmed (auto-confirmed, không cần lễ tân duyệt vì BS đã quyết),
    /// có booking_code, kế thừa thông tin BN + BS hiện tại. Increment quota.
    /// </summary>
    Task<ScheduleFollowUpResult> ScheduleFollowUpAsync(
        Guid currentApptId, DateOnly followUpDate, string session, Guid doctorUserId);
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

        if (input.Session != Constants.SessionMorning && input.Session != Constants.SessionAfternoon)
            return new BookingResult { Success = false, ErrorMessage = "Buổi khám không hợp lệ." };

        var aDate = DateOnly.FromDateTime(input.AppointmentDate);
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (aDate < today)
            return new BookingResult { Success = false, ErrorMessage = "Ngày khám không được trong quá khứ." };
        if ((aDate.DayNumber - today.DayNumber) > Constants.MaxDaysAhead)
            return new BookingResult { Success = false, ErrorMessage = $"Ngày khám không vượt quá {Constants.MaxDaysAhead} ngày." };

        // Quy trình chuẩn TTYT phường: mọi BN đặt lịch tổng quát vào "Khoa Khám bệnh"
        // (sàng lọc đa khoa). Lễ tân tiếp nhận sẽ phân BN vào ClinicRoom phù hợp dựa
        // trên triệu chứng. BN không chọn khoa/phòng cụ thể.
        var khoaKhamBenh = await _db.Departments.FirstOrDefaultAsync(d =>
            d.SiteId == siteId && d.Alias == "khoa-kham-benh" && d.ActiveFlag == 1);
        if (khoaKhamBenh == null)
            return new BookingResult { Success = false, ErrorMessage = "Hệ thống chưa khởi tạo Khoa Khám bệnh — liên hệ quản trị viên." };

        // Chống đặt lịch trùng cùng buổi cùng ngày — bệnh nhân đã login
        if (patientUserId.HasValue)
        {
            var dup = await _db.Appointments.AnyAsync(a =>
                a.PatientUserId == patientUserId &&
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
            DepartmentId    = khoaKhamBenh.Id,
            DepartmentName  = khoaKhamBenh.NameL,
            ClinicRoomId    = null, // Lễ tân sẽ phân phòng sau khi tiếp nhận triệu chứng
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

    public async Task<List<AppointmentRow>> GetByPhoneAsync(string phone, Guid siteId)
    {
        if (string.IsNullOrWhiteSpace(phone)) return new List<AppointmentRow>();
        var p = phone.Trim();
        var list = await _db.Appointments
            .Where(a => a.SiteId == siteId && a.PatientPhone == p)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.CreatedDate)
            .ToListAsync();
        return list.Select(MapRow).ToList();
    }

    public async Task<List<AppointmentRow>> GetByDateAsync(DateOnly date, Guid siteId)
    {
        var list = await _db.Appointments
            .Where(a => a.SiteId == siteId && a.AppointmentDate == date)
            .OrderBy(a => a.Session).ThenBy(a => a.Status).ThenBy(a => a.CreatedDate)
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

                // ===== TẦNG 1: dept-level quota (cap tổng cho cả khoa) =====
                var deptQuota = await _db.AppointmentQuota
                    .FirstOrDefaultAsync(q => q.DepartmentId == deptId
                                           && q.DoctorId == null
                                           && q.ApptDate == apptDate
                                           && q.Session == sess);
                if (deptQuota == null)
                {
                    deptQuota = new AppointmentQuotum
                    {
                        Id           = Guid.NewGuid(),
                        DepartmentId = deptId,
                        DoctorId     = null,
                        ApptDate     = apptDate,
                        Session      = sess,
                        MaxCount     = Constants.DefaultQuotaPerSession,
                        BookedCount  = Math.Max(0, delta),
                        CreatedDate  = DateTime.Now
                    };
                    _db.AppointmentQuota.Add(deptQuota);
                }
                else
                {
                    // Khi confirm: từ chối nếu vượt quota max của KHOA (cap trên cùng)
                    if (delta > 0 && deptQuota.BookedCount + delta > deptQuota.MaxCount)
                        return UpdateStatusResult.Fail($"Khoa đã hết suất buổi này ({deptQuota.BookedCount}/{deptQuota.MaxCount}).");
                    deptQuota.BookedCount = deptQuota.BookedCount + delta;
                    if (deptQuota.BookedCount < 0) deptQuota.BookedCount = 0;
                    deptQuota.LuUpdated = DateTime.Now;
                }

                // ===== TẦNG 2: doctor-level quota (chỉ áp dụng nếu appt đã phân BS) =====
                // Đảm bảo: BS có lịch trực ngày + ca tương ứng. BS không trực thì không cho confirm.
                // Khi cancel/reject từ confirmed có DoctorId, decrement BS-quota về.
                if (appt.DoctorId.HasValue)
                {
                    var doctorId = appt.DoctorId.Value;
                    var docQuota = await _db.AppointmentQuota
                        .FirstOrDefaultAsync(q => q.DepartmentId == deptId
                                               && q.DoctorId == doctorId
                                               && q.ApptDate == apptDate
                                               && q.Session == sess);

                    // Default MaxCount cho BS-quota: lấy từ DoctorSchedule.MaxPatients (vd: BS senior 15 / junior 8)
                    int defaultDocMax = Constants.DefaultQuotaPerSession;
                    if (delta > 0 && docQuota == null)
                    {
                        var weekday = apptDate.DayOfWeek == DayOfWeek.Sunday
                            ? (byte)1 : (byte)((int)apptDate.DayOfWeek + 1);
                        var sched = await _db.DoctorSchedules
                            .FirstOrDefaultAsync(s => s.DoctorId == doctorId
                                                   && s.Weekday == weekday
                                                   && s.Session == sess
                                                   && s.ActiveFlag == 1
                                                   && s.ValidFrom <= apptDate
                                                   && (s.ValidTo == null || s.ValidTo >= apptDate));
                        if (sched == null)
                            return UpdateStatusResult.Fail($"Bác sĩ không có lịch trực vào ngày {apptDate:dd/MM/yyyy} buổi {sess}.");

                        defaultDocMax = sched.MaxPatients ?? Constants.DefaultQuotaPerSession;
                    }

                    if (docQuota == null)
                    {
                        docQuota = new AppointmentQuotum
                        {
                            Id           = Guid.NewGuid(),
                            DepartmentId = deptId,
                            DoctorId     = doctorId,
                            ApptDate     = apptDate,
                            Session      = sess,
                            MaxCount     = defaultDocMax,
                            BookedCount  = Math.Max(0, delta),
                            CreatedDate  = DateTime.Now
                        };
                        _db.AppointmentQuota.Add(docQuota);
                    }
                    else
                    {
                        if (delta > 0 && docQuota.BookedCount + delta > docQuota.MaxCount)
                            return UpdateStatusResult.Fail($"Bác sĩ đã hết suất buổi này ({docQuota.BookedCount}/{docQuota.MaxCount}).");
                        docQuota.BookedCount = docQuota.BookedCount + delta;
                        if (docQuota.BookedCount < 0) docQuota.BookedCount = 0;
                        docQuota.LuUpdated = DateTime.Now;
                    }
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

        // Chỉ cho phép phân/đổi BS khi appointment chưa confirm hoặc đã reschedule (chờ duyệt lại).
        // Sau khi confirmed → BS-quota đã trừ; đổi BS bằng cách này sẽ phá quota.
        // Lễ tân muốn đổi BS post-confirm phải reject + tạo lại lịch (workflow chuẩn ISTQB).
        var status = appt.Status ?? "";
        if (status != Constants.ApptPending && status != Constants.ApptRescheduled)
            return false;

        // Cross-site guard: nếu doctorId được truyền vào, phải thuộc cùng site với appointment.
        if (doctorId.HasValue && appt.SiteId.HasValue)
        {
            var docInSite = await (from d in _db.Doctors
                                   join dep in _db.Departments on d.DepartmentId equals dep.Id
                                   where d.Id == doctorId.Value
                                      && dep.SiteId == appt.SiteId.Value
                                      && d.ActiveFlag == 1
                                      && dep.ActiveFlag == 1
                                   select d.Id).AnyAsync();
            if (!docInSite) return false;
        }

        appt.DoctorId  = doctorId;
        appt.LuUpdated = DateTime.Now;
        appt.LuUserId  = staffUserId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ScheduleFollowUpResult> ScheduleFollowUpAsync(
        Guid currentApptId, DateOnly followUpDate, string session, Guid doctorUserId)
    {
        if (string.IsNullOrEmpty(session)) return ScheduleFollowUpResult.Fail("Buổi khám không hợp lệ.");
        if (session != Constants.SessionMorning && session != Constants.SessionAfternoon)
            return ScheduleFollowUpResult.Fail("Buổi khám phải là sáng hoặc chiều.");
        if (followUpDate < DateOnly.FromDateTime(DateTime.Today))
            return ScheduleFollowUpResult.Fail("Ngày tái khám không được trong quá khứ.");
        if (followUpDate > DateOnly.FromDateTime(DateTime.Today.AddDays(Constants.MaxDaysAhead * 6)))
            return ScheduleFollowUpResult.Fail($"Ngày tái khám không quá {Constants.MaxDaysAhead * 6} ngày.");

        var current = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == currentApptId);
        if (current == null) return ScheduleFollowUpResult.Fail("Lịch khám hiện tại không tồn tại.");
        if (!current.PatientUserId.HasValue)
            return ScheduleFollowUpResult.Fail("BN chưa có tài khoản — không thể đặt lịch tái khám tự động.");
        if (!current.DepartmentId.HasValue)
            return ScheduleFollowUpResult.Fail("Lịch hiện tại thiếu khoa — không thể tái khám.");
        if (!current.DoctorId.HasValue)
            return ScheduleFollowUpResult.Fail("Lịch hiện tại thiếu BS — không thể đặt tái khám.");

        // Sinh booking code dạng KMyymmdd<S|C><6hex>
        var sessTag = session == Constants.SessionMorning ? "S" : "C";
        var rand    = Guid.NewGuid().ToString("N")[..6].ToUpper();
        var dateStr = followUpDate.ToString("yyMMdd");
        var bookingCode = $"KM{dateStr}{sessTag}{rand}";

        var newAppt = new Appointment
        {
            Id              = Guid.NewGuid(),
            PatientUserId   = current.PatientUserId,
            PatientName     = current.PatientName,
            PatientPhone    = current.PatientPhone,
            PatientEmail    = current.PatientEmail,
            DepartmentId    = current.DepartmentId,
            DepartmentName  = current.DepartmentName,
            DoctorId        = current.DoctorId,           // BS hiện tại theo dõi tiếp
            ClinicRoomId    = current.ClinicRoomId,        // BS thường ở cùng phòng
            AppointmentDate = followUpDate,
            Session         = session,
            Reason          = $"Tái khám theo chỉ định BS sau lịch {current.BookingCode ?? current.Id.ToString("N")[..8]}.",
            Status          = Constants.ApptConfirmed,    // BS đã quyết → confirmed luôn
            BookingCode     = bookingCode,
            CheckedIn       = false,
            IsEmergency     = false,
            SiteId          = current.SiteId,
            CreatedDate     = DateTime.Now,
            LuUpdated       = DateTime.Now,
            LuUserId        = doctorUserId,
        };
        _db.Appointments.Add(newAppt);

        // Increment quota — cả dept-level và doctor-level (giống flow confirm bình thường)
        var deptId = current.DepartmentId.Value;
        async Task IncQuota(Guid? docId)
        {
            var q = await _db.AppointmentQuota.FirstOrDefaultAsync(x =>
                x.DepartmentId == deptId
                && x.DoctorId == docId
                && x.ApptDate == followUpDate
                && x.Session == session);
            if (q == null)
            {
                int defaultMax = Constants.DefaultQuotaPerSession;
                if (docId.HasValue)
                {
                    var weekday = followUpDate.DayOfWeek == DayOfWeek.Sunday
                        ? (byte)1 : (byte)((int)followUpDate.DayOfWeek + 1);
                    var sched = await _db.DoctorSchedules.FirstOrDefaultAsync(s =>
                        s.DoctorId == docId.Value && s.Weekday == weekday && s.Session == session
                        && s.ActiveFlag == 1 && s.ValidFrom <= followUpDate
                        && (s.ValidTo == null || s.ValidTo >= followUpDate));
                    if (sched == null)
                        throw new InvalidOperationException($"BS không có lịch trực ngày {followUpDate:dd/MM/yyyy} buổi {session}.");
                    defaultMax = sched.MaxPatients ?? Constants.DefaultQuotaPerSession;
                }
                _db.AppointmentQuota.Add(new AppointmentQuotum
                {
                    Id = Guid.NewGuid(), DepartmentId = deptId, DoctorId = docId,
                    ApptDate = followUpDate, Session = session,
                    MaxCount = defaultMax, BookedCount = 1, CreatedDate = DateTime.Now,
                });
            }
            else
            {
                if (q.BookedCount + 1 > q.MaxCount)
                    throw new InvalidOperationException(docId.HasValue
                        ? $"BS đã hết suất buổi này ({q.BookedCount}/{q.MaxCount})."
                        : $"Khoa đã hết suất buổi này ({q.BookedCount}/{q.MaxCount}).");
                q.BookedCount += 1;
                q.LuUpdated = DateTime.Now;
            }
        }
        try
        {
            await IncQuota(null);                       // dept-level
            await IncQuota(current.DoctorId.Value);     // doctor-level
            await _db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            return ScheduleFollowUpResult.Fail(ex.Message);
        }

        return new ScheduleFollowUpResult
        {
            Success       = true,
            AppointmentId = newAppt.Id,
            BookingCode   = bookingCode,
            FollowUpDate  = followUpDate,
        };
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
        CreatedDate     = a.CreatedDate,
        ClinicRoomId    = a.ClinicRoomId,
        IsEmergency     = a.IsEmergency,
    };
}
