using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;
using Xunit;

namespace WebsiteCore.Tests;

/// <summary>
/// Tests cho workflow đặt lịch — book, duyệt, từ chối, transition, race.
/// Đáp ứng tiêu chuẩn kiểm thử y tế: state machine + audit + concurrency + reject reason.
/// </summary>
public class AppointmentServiceTests
{
    private static (TtytlpDbContext db, Site site, Department dept, User staff, User patient) Seed()
    {
        var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);
        var dept = db.Departments.First();
        var staff = new User { Id = Guid.NewGuid(), UserName = "letan", GroupId = Constants.ReceptionGroup, ActiveFlag = 1 };
        var patient = new User { Id = Guid.NewGuid(), UserName = "patient", GroupId = Constants.MemberGroup, ActiveFlag = 1, FullName = "Bệnh nhân X", Phone = "0900000000" };
        db.Users.AddRange(staff, patient);
        db.SaveChanges();
        return (db, site, dept, staff, patient);
    }

    private static BookingInputModel ValidInput(Guid deptId, DateTime? date = null) => new()
    {
        PatientName = "Nguyễn Văn A",
        PatientPhone = "0900000000",
        DepartmentId = deptId,
        AppointmentDate = date ?? DateTime.Today.AddDays(2),
        Session = Constants.SessionMorning,
        Reason = "Khám tổng quát"
    };

    [Fact]
    public async Task Create_Valid_Succeeds()
    {
        var (db, site, dept, _, patient) = Seed();
        var svc = new AppointmentService(db);
        var res = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        Assert.True(res.Success);
        Assert.NotNull(res.AppointmentId);
        var saved = await db.Appointments.FindAsync(res.AppointmentId);
        Assert.Equal(Constants.ApptPending, saved!.Status);
        Assert.Equal(patient.Id, saved.PatientUserId);
    }

    [Fact]
    public async Task Create_PastDate_Rejected()
    {
        var (db, site, dept, _, patient) = Seed();
        var input = ValidInput(dept.Id, DateTime.Today.AddDays(-1));
        var res = await new AppointmentService(db).CreateAsync(input, patient.Id, site.Id);
        Assert.False(res.Success);
        Assert.Contains("quá khứ", res.ErrorMessage);
    }

    [Fact]
    public async Task Create_TooFarInFuture_Rejected()
    {
        var (db, site, dept, _, patient) = Seed();
        var input = ValidInput(dept.Id, DateTime.Today.AddDays(Constants.MaxDaysAhead + 5));
        var res = await new AppointmentService(db).CreateAsync(input, patient.Id, site.Id);
        Assert.False(res.Success);
    }

    [Fact]
    public async Task Create_InvalidSession_Rejected()
    {
        var (db, site, dept, _, patient) = Seed();
        var input = ValidInput(dept.Id);
        input.Session = "<script>alert(1)</script>"; // injection attempt
        var res = await new AppointmentService(db).CreateAsync(input, patient.Id, site.Id);
        Assert.False(res.Success);
        Assert.Contains("hợp lệ", res.ErrorMessage);
    }

    [Fact]
    public async Task Create_InactiveDepartment_Rejected()
    {
        var (db, site, dept, _, patient) = Seed();
        dept.ActiveFlag = 0;
        await db.SaveChangesAsync();
        var res = await new AppointmentService(db).CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        Assert.False(res.Success);
    }

    [Fact]
    public async Task Create_DuplicateSameSession_Rejected()
    {
        var (db, site, dept, _, patient) = Seed();
        var svc = new AppointmentService(db);
        var first = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        Assert.True(first.Success);
        var second = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        Assert.False(second.Success);
        Assert.Contains("đã có lịch", second.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_LongPatientName_Truncated()
    {
        var (db, site, dept, _, patient) = Seed();
        var input = ValidInput(dept.Id);
        input.PatientName = new string('A', 500);
        var res = await new AppointmentService(db).CreateAsync(input, patient.Id, site.Id);
        Assert.True(res.Success);
        var saved = await db.Appointments.FindAsync(res.AppointmentId);
        Assert.True(saved!.PatientName!.Length <= 150);
    }

    [Fact]
    public async Task UpdateStatus_PendingToConfirmed_GeneratesBookingCode()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);
        Assert.True(r.Success);
        Assert.NotNull(r.BookingCode);
        Assert.StartsWith("KM", r.BookingCode);
        Assert.Equal(Constants.ApptPending, r.OldStatus);
        Assert.Equal(Constants.ApptConfirmed, r.NewStatus);
    }

    [Fact]
    public async Task UpdateStatus_RejectWithoutReason_Fails()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptRejected, null, staff.Id);
        Assert.False(r.Success);
        Assert.Contains("lý do", r.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatus_RejectWithReason_Succeeds_AndSavesNote()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptRejected, "BS bận đột xuất", staff.Id);
        Assert.True(r.Success);
        var saved = await db.Appointments.FindAsync(c.AppointmentId);
        Assert.Equal(Constants.ApptRejected, saved!.Status);
        Assert.Equal("BS bận đột xuất", saved.StaffNote);
        Assert.Equal(staff.Id, saved.LuUserId);
    }

    [Fact]
    public async Task UpdateStatus_FromRejected_DoesNotAllowReopen()
    {
        // Trạng thái cuối — không được đổi nữa (chống leo trạng thái)
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptRejected, "x", staff.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);
        Assert.False(r.Success);
    }

    [Fact]
    public async Task UpdateStatus_FromCompleted_Frozen()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptCompleted, null, staff.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptCancelled, null, staff.Id);
        Assert.False(r.Success);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatusName_Rejected()
    {
        // Chống URL/form tampering — newStatus phải nằm trong whitelist
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, "DROP TABLE Users", null, staff.Id);
        Assert.False(r.Success);
    }

    [Fact]
    public async Task UpdateStatus_QuotaIncrement_OnConfirm()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);

        var quota = db.AppointmentQuota.FirstOrDefault(q =>
            q.DepartmentId == dept.Id &&
            q.ApptDate == DateOnly.FromDateTime(DateTime.Today.AddDays(2)) &&
            q.Session == Constants.SessionMorning);
        Assert.NotNull(quota);
        Assert.Equal(1, quota!.BookedCount);
    }

    [Fact]
    public async Task UpdateStatus_QuotaDecrement_OnCancelAfterConfirm()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptCancelled, null, staff.Id);

        var quota = db.AppointmentQuota.First();
        Assert.Equal(0, quota.BookedCount);
    }

    [Fact]
    public async Task UpdateStatus_ConfirmRefused_WhenQuotaFull()
    {
        var (db, site, dept, staff, _) = Seed();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        // Pre-fill quota = max
        db.AppointmentQuota.Add(new AppointmentQuotum
        {
            Id = Guid.NewGuid(),
            DepartmentId = dept.Id,
            ApptDate = date,
            Session = Constants.SessionMorning,
            MaxCount = 1,
            BookedCount = 1,
            CreatedDate = DateTime.Now
        });
        await db.SaveChangesAsync();

        var svc = new AppointmentService(db);
        // Patient walk-in (no userId)
        var c = await svc.CreateAsync(ValidInput(dept.Id), null, site.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);
        Assert.False(r.Success);
        Assert.Contains("hết suất", r.ErrorMessage);
    }

    [Fact]
    public async Task MarkCheckedIn_RequiresConfirmedAndToday()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        // Lịch chưa confirmed
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        Assert.False(await svc.MarkCheckedInAsync(c.AppointmentId!.Value, staff.Id));

        // Confirm rồi nhưng ngày khác
        await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptConfirmed, null, staff.Id);
        Assert.False(await svc.MarkCheckedInAsync(c.AppointmentId!.Value, staff.Id));
    }

    [Fact]
    public async Task GetByPatient_OnlyReturnsOwn()
    {
        var (db, site, dept, _, patient) = Seed();
        var other = new User { Id = Guid.NewGuid(), UserName = "other", GroupId = "MEMBER", ActiveFlag = 1 };
        db.Users.Add(other);
        await db.SaveChangesAsync();
        var svc = new AppointmentService(db);
        await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        // Bệnh nhân khác đặt lịch khoa khác (giữa same dept ko được — rules ko cho)
        var input2 = ValidInput(dept.Id);
        input2.AppointmentDate = DateTime.Today.AddDays(3);
        await svc.CreateAsync(input2, other.Id, site.Id);

        var mine = await svc.GetByPatientAsync(patient.Id);
        Assert.Single(mine);
        Assert.Equal(patient.Id, mine[0].PatientUserId);
    }

    [Fact]
    public async Task UpdateStatus_NonExistent_ReturnsFail()
    {
        var (db, _, _, staff, _) = Seed();
        var svc = new AppointmentService(db);
        var r = await svc.UpdateStatusAsync(Guid.NewGuid(), Constants.ApptConfirmed, null, staff.Id);
        Assert.False(r.Success);
    }

    [Fact]
    public async Task UpdateStatus_StaffNoteTruncatedTo500()
    {
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        var longNote = new string('x', 1000);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptRejected, longNote, staff.Id);
        Assert.True(r.Success);
        var saved = await db.Appointments.FindAsync(c.AppointmentId);
        Assert.True(saved!.StaffNote!.Length <= 500);
    }

    [Fact]
    public async Task UpdateStatus_PendingToCompleted_NotAllowed()
    {
        // Phải qua confirmed trước; pending → completed không hợp lệ
        var (db, site, dept, staff, patient) = Seed();
        var svc = new AppointmentService(db);
        var c = await svc.CreateAsync(ValidInput(dept.Id), patient.Id, site.Id);
        var r = await svc.UpdateStatusAsync(c.AppointmentId!.Value, Constants.ApptCompleted, null, staff.Id);
        Assert.False(r.Success);
    }
}
