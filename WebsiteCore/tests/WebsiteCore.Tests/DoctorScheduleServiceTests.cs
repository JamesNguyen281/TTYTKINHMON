using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;
using Xunit;

namespace WebsiteCore.Tests;

/// <summary>
/// Tests cho auto-gen lịch trực BS hàng tháng:
/// - Idempotency (chạy 2 lần không tạo trùng)
/// - Site scoping (BS site khác bỏ qua)
/// - Doctor active/inactive filter
/// - Department active filter
/// - Distribution: 5 weekday × 2 session = 10 slot per BS
/// </summary>
public class DoctorScheduleServiceTests
{
    private static (TtytlpDbContext db, Site site, Department dept, Doctor doc1, Doctor doc2) Seed()
    {
        var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);
        var dept = db.Departments.First(d => d.SiteId == site.Id);

        var doc1 = new Doctor { Id = Guid.NewGuid(), NameL = "BS A", DepartmentId = dept.Id, ActiveFlag = 1, Ord = 1 };
        var doc2 = new Doctor { Id = Guid.NewGuid(), NameL = "BS B", DepartmentId = dept.Id, ActiveFlag = 1, Ord = 2 };
        db.Doctors.AddRange(doc1, doc2);
        db.SaveChanges();
        return (db, site, dept, doc1, doc2);
    }

    [Fact]
    public async Task GenerateMonthly_CreatesTenSlotsPerDoctor()
    {
        var (db, site, _, _, _) = Seed();
        var svc = new DoctorScheduleService(db);
        // 2 BS × 10 slot = 20
        var rs = await svc.GenerateMonthlyScheduleAsync(2026, 6, site.Id, null);
        Assert.Equal(20, rs.Created);
        Assert.Equal(2, rs.DoctorsProcessed);
        Assert.Equal(0, rs.SkippedExisting);

        // Mỗi BS đúng 10 slot (5 weekday × 2 session)
        var allSchedules = db.DoctorSchedules.ToList();
        Assert.Equal(20, allSchedules.Count);
        Assert.All(allSchedules, s =>
        {
            Assert.True(s.Weekday >= 2 && s.Weekday <= 6, "Phải là weekday Mon-Fri (2-6)");
            Assert.True(s.Session == Constants.SessionMorning || s.Session == Constants.SessionAfternoon);
            Assert.Equal(new DateOnly(2026, 6, 1), s.ValidFrom);
            Assert.Equal(new DateOnly(2026, 6, 30), s.ValidTo);
            Assert.Equal(1, s.ActiveFlag);
        });
    }

    [Fact]
    public async Task GenerateMonthly_Idempotent_RunningTwiceDoesNotDuplicate()
    {
        var (db, site, _, _, _) = Seed();
        var svc = new DoctorScheduleService(db);
        var first = await svc.GenerateMonthlyScheduleAsync(2026, 7, site.Id, null);
        var second = await svc.GenerateMonthlyScheduleAsync(2026, 7, site.Id, null);

        Assert.Equal(20, first.Created);
        Assert.Equal(0, second.Created);
        Assert.Equal(2, second.SkippedExisting);
        // Tổng số schedule trong DB không thay đổi sau lần 2
        Assert.Equal(20, db.DoctorSchedules.Count());
    }

    [Fact]
    public async Task GenerateMonthly_DifferentMonth_DoesNotConflict()
    {
        var (db, site, _, _, _) = Seed();
        var svc = new DoctorScheduleService(db);
        await svc.GenerateMonthlyScheduleAsync(2026, 6, site.Id, null);
        var rs = await svc.GenerateMonthlyScheduleAsync(2026, 7, site.Id, null);
        // Tháng 7 vẫn tạo được dù tháng 6 đã có
        Assert.Equal(20, rs.Created);
        Assert.Equal(0, rs.SkippedExisting);
        Assert.Equal(40, db.DoctorSchedules.Count());
    }

    [Fact]
    public async Task GenerateMonthly_SiteScoping_OtherSiteDoctorsIgnored()
    {
        var (db, site, _, _, _) = Seed();
        var otherSiteId = Guid.NewGuid();
        var svc = new DoctorScheduleService(db);
        var rs = await svc.GenerateMonthlyScheduleAsync(2026, 6, otherSiteId, null);
        Assert.Equal(0, rs.DoctorsProcessed);
        Assert.Equal(0, rs.Created);
    }

    [Fact]
    public async Task GenerateMonthly_InactiveDoctor_Skipped()
    {
        var (db, site, _, doc1, _) = Seed();
        doc1.ActiveFlag = 0;
        db.SaveChanges();
        var svc = new DoctorScheduleService(db);
        var rs = await svc.GenerateMonthlyScheduleAsync(2026, 6, site.Id, null);
        // Chỉ còn 1 BS active → 10 slot
        Assert.Equal(1, rs.DoctorsProcessed);
        Assert.Equal(10, rs.Created);
    }

    [Fact]
    public async Task GenerateMonthly_InactiveDepartment_DoctorsIgnored()
    {
        var (db, site, dept, _, _) = Seed();
        dept.ActiveFlag = 0;
        db.SaveChanges();
        var svc = new DoctorScheduleService(db);
        var rs = await svc.GenerateMonthlyScheduleAsync(2026, 6, site.Id, null);
        Assert.Equal(0, rs.DoctorsProcessed);
        Assert.Equal(0, rs.Created);
    }

    [Theory]
    [InlineData(2019, 1)]   // year quá thấp
    [InlineData(2101, 1)]   // year quá cao
    [InlineData(2026, 0)]   // month invalid
    [InlineData(2026, 13)]  // month invalid
    public async Task GenerateMonthly_InvalidYearMonth_ReturnsEmpty(int year, int month)
    {
        var (db, site, _, _, _) = Seed();
        var svc = new DoctorScheduleService(db);
        var rs = await svc.GenerateMonthlyScheduleAsync(year, month, site.Id, null);
        Assert.Equal(0, rs.Created);
        Assert.Equal(0, rs.DoctorsProcessed);
        Assert.Empty(db.DoctorSchedules);
    }

    [Fact]
    public async Task GenerateMonthly_SetsAuditFields()
    {
        var (db, site, _, _, _) = Seed();
        var staffId = Guid.NewGuid();
        var svc = new DoctorScheduleService(db);
        await svc.GenerateMonthlyScheduleAsync(2026, 6, site.Id, staffId);
        var first = db.DoctorSchedules.First();
        Assert.Equal(staffId, first.CreatedBy);
        Assert.Contains("Auto-gen tháng 06/2026", first.Note);
        Assert.True(first.CreatedDate >= DateTime.Today);
    }
}
