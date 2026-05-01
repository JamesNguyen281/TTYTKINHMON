using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface ISiteService
{
    Task<Site?> GetCurrentAsync();
    Task<Site?> GetByIdAsync(Guid id);
    Task<List<Site>> GetAllActiveAsync();
    Task<List<Site>> GetAllAsync();
    Task UpdateAsync(Site s);
    Task<string?> GetDashboardImageAsync(Guid siteId);
    Task UpdateDashboardImageAsync(Guid siteId, string relativePath);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id);
}

public class SiteService : ISiteService
{
    private readonly TtytlpDbContext _db;
    public SiteService(TtytlpDbContext db) => _db = db;

    public Task<Site?> GetCurrentAsync() =>
        _db.Sites.Where(s => s.ActiveFlag == 1).OrderBy(s => s.Ord).FirstOrDefaultAsync();

    public Task<Site?> GetByIdAsync(Guid id) =>
        _db.Sites.FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Site>> GetAllActiveAsync() =>
        _db.Sites.Where(s => s.ActiveFlag == 1).OrderBy(s => s.Ord).ToListAsync();

    public Task<List<Site>> GetAllAsync() =>
        _db.Sites.OrderBy(s => s.Ord).ToListAsync();

    public async Task UpdateAsync(Site s)
    {
        // Reload-then-apply — KHÔNG trust Id / CreatedDate từ form
        var existing = await _db.Sites.FirstOrDefaultAsync(x => x.Id == s.Id);
        if (existing == null) return;
        existing.NameCompanyL    = s.NameCompanyL;
        existing.NameCompanyE    = s.NameCompanyE;
        existing.AddressL        = s.AddressL;
        existing.AddressE        = s.AddressE;
        existing.Phone           = s.Phone;
        existing.MobilePhone     = s.MobilePhone;
        existing.Hotline         = s.Hotline;
        existing.EmergencyNumber = s.EmergencyNumber;
        existing.Fax             = s.Fax;
        existing.Email           = s.Email;
        existing.TimeOpen        = s.TimeOpen;
        existing.Map             = s.Map;
        existing.MetaDescription = s.MetaDescription;
        existing.MetaKeyword     = s.MetaKeyword;
        existing.LogoUrl         = s.LogoUrl;
        existing.Favicon         = s.Favicon;
        existing.Ord             = s.Ord;
        existing.ActiveFlag      = s.ActiveFlag;
        await _db.SaveChangesAsync();
    }

    public Task<string?> GetDashboardImageAsync(Guid siteId) =>
        _db.Sites.Where(x => x.Id == siteId).Select(x => x.DashboardImage).FirstOrDefaultAsync();

    public async Task UpdateDashboardImageAsync(Guid siteId, string relativePath)
    {
        var s = await _db.Sites.FirstOrDefaultAsync(x => x.Id == siteId);
        if (s == null) return;
        s.DashboardImage = relativePath;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Hard delete một site — chỉ cho phép khi KHÔNG còn FK reference từ News/User/Category/Department/...
    /// Trả về (false, lý do) nếu có dependent data; ngược lại xoá row + (true, null).
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id)
    {
        var s = await _db.Sites.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return (false, "Site không tồn tại.");

        // Reject nếu là site đang active duy nhất — phải còn ít nhất 1 site active
        var otherActive = await _db.Sites.CountAsync(x => x.Id != id && x.ActiveFlag == 1);
        if (s.ActiveFlag == 1 && otherActive == 0)
            return (false, "Không thể xoá site active duy nhất. Đặt active=0 hoặc kích hoạt site khác trước.");

        // Đếm reference từ các bảng phụ thuộc
        var refs = new List<string>();
        var news      = await _db.News.CountAsync(x => x.SiteId == id);              if (news > 0)      refs.Add($"News ({news})");
        var cats      = await _db.Categories.CountAsync(x => x.SiteId == id);        if (cats > 0)      refs.Add($"Category ({cats})");
        var depts     = await _db.Departments.CountAsync(x => x.SiteId == id);       if (depts > 0)     refs.Add($"Department ({depts})");
        var appts     = await _db.Appointments.CountAsync(x => x.SiteId == id);      if (appts > 0)     refs.Add($"Appointment ({appts})");
        var slides    = await _db.Slides.CountAsync(x => x.SiteId == id);            if (slides > 0)    refs.Add($"Slide ({slides})");
        var videos    = await _db.Videos.CountAsync(x => x.SiteId == id);            if (videos > 0)    refs.Add($"Video ({videos})");
        var partners  = await _db.Partners.CountAsync(x => x.SiteId == id);          if (partners > 0)  refs.Add($"Partner ({partners})");
        var questions = await _db.Questions.CountAsync(x => x.SiteId == id);         if (questions > 0) refs.Add($"Question ({questions})");
        var docs      = await _db.Documents.CountAsync(x => x.SiteId == id);         if (docs > 0)      refs.Add($"Document ({docs})");

        if (refs.Count > 0)
            return (false, $"Site còn dữ liệu phụ thuộc ({string.Join(", ", refs)}) — không thể xoá. Hãy dọn các bảng phụ trước.");

        _db.Sites.Remove(s);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
