using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IScheduleChangeRequestService
{
    Task<Guid> CreateAsync(ScheduleChangeRequest req);
    Task<List<ScheduleChangeRequest>> GetByDoctorAsync(Guid doctorId);
    Task<List<ScheduleChangeRequest>> GetByStatusAsync(string? status = null);
    Task<ScheduleChangeRequest?> GetByIdAsync(Guid id);
    Task<bool> ProcessAsync(Guid id, string newStatus, string? response, Guid processedBy);
    Task<int> CountPendingAsync();
}

public class ScheduleChangeRequestService : IScheduleChangeRequestService
{
    private readonly TtytlpDbContext _db;
    public ScheduleChangeRequestService(TtytlpDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(ScheduleChangeRequest req)
    {
        if (req.Id == Guid.Empty) req.Id = Guid.NewGuid();
        if (req.CreatedDate == default) req.CreatedDate = DateTime.Now;
        if (string.IsNullOrWhiteSpace(req.Status)) req.Status = "pending";
        // Cap dài chống DoS
        if (req.Reason.Length > 2000) req.Reason = req.Reason.Substring(0, 2000);
        _db.ScheduleChangeRequests.Add(req);
        await _db.SaveChangesAsync();
        return req.Id;
    }

    public Task<List<ScheduleChangeRequest>> GetByDoctorAsync(Guid doctorId) =>
        _db.ScheduleChangeRequests
           .Where(r => r.DoctorId == doctorId)
           .OrderByDescending(r => r.CreatedDate)
           .ToListAsync();

    public Task<List<ScheduleChangeRequest>> GetByStatusAsync(string? status = null)
    {
        var q = _db.ScheduleChangeRequests.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(r => r.Status == status);
        return q.OrderByDescending(r => r.CreatedDate).ToListAsync();
    }

    public Task<ScheduleChangeRequest?> GetByIdAsync(Guid id) =>
        _db.ScheduleChangeRequests.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<bool> ProcessAsync(Guid id, string newStatus, string? response, Guid processedBy)
    {
        var r = await _db.ScheduleChangeRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return false;
        if (r.Status != "pending") return false; // chỉ cho duyệt đơn pending — chống double-process
        r.Status = newStatus;
        r.AdminResponse = string.IsNullOrWhiteSpace(response) ? null
                          : (response.Length > 2000 ? response.Substring(0, 2000) : response);
        r.ProcessedBy = processedBy;
        r.ProcessedDate = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<int> CountPendingAsync() =>
        _db.ScheduleChangeRequests.CountAsync(r => r.Status == "pending");
}
