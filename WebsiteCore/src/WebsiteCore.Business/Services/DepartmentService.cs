using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IDepartmentService
{
    Task<List<Department>> GetActiveBySiteAsync(Guid siteId);
    Task<List<Department>> GetAllBySiteAsync(Guid siteId);
    /// <summary>
    /// Trả các khoa có thể nhận BN khám (IsClinicalDept=true) — dùng cho dropdown đặt lịch online.
    /// Loại trừ Khoa Khám bệnh (wrapper), Khoa Xét nghiệm (cận lâm sàng), Khoa Dược, Khoa Cấp cứu (đến trực tiếp),
    /// Khoa Y tế công cộng, Phòng Dân số… những khoa không phục vụ khám trực tiếp.
    /// </summary>
    Task<List<Department>> GetClinicalBySiteAsync(Guid siteId);
    Task<Department?> GetByIdAsync(Guid id);
    Task<Department?> GetByAliasAsync(string alias);
    Task<Guid> CreateAsync(Department dept, Guid siteId);
    Task<bool> UpdateAsync(Department dept);
    Task<bool> DeleteAsync(Guid id);
}

public class DepartmentService : IDepartmentService
{
    private readonly TtytlpDbContext _db;
    public DepartmentService(TtytlpDbContext db) => _db = db;

    public Task<List<Department>> GetActiveBySiteAsync(Guid siteId) =>
        _db.Departments
           .Where(d => d.SiteId == siteId && d.ActiveFlag == 1)
           .OrderBy(d => d.Ord)
           .ThenBy(d => d.NameL)
           .ToListAsync();

    public Task<List<Department>> GetClinicalBySiteAsync(Guid siteId) =>
        _db.Departments
           .Where(d => d.SiteId == siteId && d.ActiveFlag == 1 && d.IsClinicalDept)
           .OrderBy(d => d.Ord)
           .ThenBy(d => d.NameL)
           .ToListAsync();

    public Task<Department?> GetByIdAsync(Guid id) =>
        _db.Departments.FirstOrDefaultAsync(d => d.Id == id);

    public Task<Department?> GetByAliasAsync(string alias) =>
        _db.Departments.FirstOrDefaultAsync(d => d.Alias == alias);

    public Task<List<Department>> GetAllBySiteAsync(Guid siteId) =>
        _db.Departments.Where(d => d.SiteId == siteId).OrderBy(d => d.Ord).ThenBy(d => d.NameL).ToListAsync();

    public async Task<Guid> CreateAsync(Department dept, Guid siteId)
    {
        dept.Id = Guid.NewGuid();
        dept.SiteId = siteId;
        dept.CreatedDate = DateTime.Now;
        if (string.IsNullOrEmpty(dept.NameE)) dept.NameE = dept.NameL;
        dept.Alias = Helpers.StringHelper.ChangeText(dept.NameL ?? string.Empty);
        if (string.IsNullOrEmpty(dept.Link)) dept.Link = "chuyen-khoa/" + dept.Alias;
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        return dept.Id;
    }

    public async Task<bool> UpdateAsync(Department dept)
    {
        var existing = await _db.Departments.FirstOrDefaultAsync(d => d.Id == dept.Id);
        if (existing == null) return false;
        existing.NameL = dept.NameL;
        existing.NameE = dept.NameE ?? dept.NameL;
        existing.DescriptionL = dept.DescriptionL;
        existing.DescriptionE = dept.DescriptionE;
        existing.DetailL = dept.DetailL;
        existing.ImagePath = dept.ImagePath;
        existing.ActiveFlag = dept.ActiveFlag;
        existing.Ord = dept.Ord;
        existing.IsClinicalDept = dept.IsClinicalDept;
        existing.LuUpdated = DateTime.Now;
        existing.Alias = Helpers.StringHelper.ChangeText(dept.NameL ?? string.Empty);
        existing.Link = "chuyen-khoa/" + existing.Alias;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var d = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return false;
        d.ActiveFlag = 0;  // soft delete
        d.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }
}
