using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string detail);
}

public class AuditService : IAuditService
{
    private readonly TtytlpDbContext _db;
    public AuditService(TtytlpDbContext db) => _db = db;

    public async Task LogAsync(Guid? userId, string action, string detail)
    {
        var row = new AuditSystem
        {
            Id                = Guid.NewGuid(),
            UserId            = userId,
            ActionDescription = action,
            ActionDetail      = detail,
            ActionDate        = DateTime.Now
        };
        _db.AuditSystems.Add(row);
        await _db.SaveChangesAsync();
    }
}
