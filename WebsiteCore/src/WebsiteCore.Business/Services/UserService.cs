using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business.Helpers;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IUserService
{
    /// <summary>Returns user nếu credentials đúng, null nếu sai.</summary>
    Task<User?> CheckLoginAsync(string userName, string password);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> UserNameExistsAsync(string userName);
    Task<Guid?> RegisterMemberAsync(RegisterViewModel vm, string? createdByIp = null);
    Task<bool> UpdateProfileAsync(Guid userId, string fullName, string phone, string? email, int? gender);
    Task<bool> UpdatePatientProfileAsync(Guid userId, PatientProfileInput input);
    /// <summary>Bác sĩ cập nhật thông tin y tế của bệnh nhân khi chẩn đoán
    /// (BloodType + Allergies + MedicalHistory). Chỉ ghi đè khi giá trị mới
    /// non-empty — giữ giá trị cũ nếu bác sĩ để trống.</summary>
    Task<bool> UpdateMedicalInfoAsync(Guid userId, string? bloodType, string? allergies, string? medicalHistory, Guid updatedBy);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}

public class UserService : IUserService
{
    private readonly TtytlpDbContext _db;

    public UserService(TtytlpDbContext db) => _db = db;

    /// <summary>Số lần nhập sai liên tiếp tối đa trước khi lockout 15 phút.</summary>
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<User?> CheckLoginAsync(string userName, string password)
    {
        var u = await _db.Users
            .FirstOrDefaultAsync(x => x.UserName == userName && x.ActiveFlag == 1);
        if (u == null) return null;

        // Lockout check — đang trong thời gian khoá
        if (u.LockoutUntil.HasValue && u.LockoutUntil.Value > DateTime.Now)
            return null;

        if (!StringHelper.VerifyPassword(password, u.Password))
        {
            // Tăng đếm failed attempts; nếu đạt ngưỡng → set LockoutUntil
            u.FailedAttempts += 1;
            if (u.FailedAttempts >= MaxFailedAttempts)
            {
                u.LockoutUntil = DateTime.Now.Add(LockoutDuration);
                u.FailedAttempts = 0; // reset counter cho lần lockout tiếp
            }
            await _db.SaveChangesAsync();
            return null;
        }

        // Login thành công — reset counter + cập nhật last_login
        bool needsSave = false;
        if (u.FailedAttempts != 0) { u.FailedAttempts = 0; needsSave = true; }
        if (u.LockoutUntil != null) { u.LockoutUntil = null; needsSave = true; }
        u.LastLogin = DateTime.Now; needsSave = true;

        // Auto-rehash: nếu password đang lưu là MD5 hoặc PBKDF2 iterations thấp → upgrade
        // (đã có plaintext trong tay sau khi verify thành công).
        if (StringHelper.NeedsRehash(u.Password))
        {
            u.Password = StringHelper.HashPassword(password);
            needsSave = true;
        }

        if (needsSave) await _db.SaveChangesAsync();
        return u;
    }

    public Task<User?> GetByUserNameAsync(string userName) =>
        _db.Users.FirstOrDefaultAsync(x => x.UserName == userName);

    public Task<User?> GetByIdAsync(Guid id) =>
        _db.Users.FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> UserNameExistsAsync(string userName) =>
        _db.Users.AnyAsync(x => x.UserName == userName);

    public async Task<Guid?> RegisterMemberAsync(RegisterViewModel vm, string? createdByIp = null)
    {
        var existing = await _db.Users.AnyAsync(x => x.UserName == vm.UserName);
        if (existing) return null;

        int? genderInt = vm.Gender switch
        {
            "male"   => 1,
            "female" => 2,
            _        => null
        };

        var u = new User
        {
            Id            = Guid.NewGuid(),
            UserName      = vm.UserName.Trim().ToLower(),
            FullName      = vm.FullName.Trim(),
            Password      = StringHelper.HashPassword(vm.Password),
            Email         = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
            Phone         = vm.Phone.Trim(),
            Gender        = genderInt,
            GroupId       = Constants.MemberGroup,
            ActiveFlag    = 1,
            CreatedDate   = DateTime.Now,
            ImagePath     = "assets/admin/images/user_none.jpg"
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        return u.Id;
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, string fullName, string phone, string? email, int? gender)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (u == null) return false;
        u.FullName = fullName.Trim();
        u.Phone = phone.Trim();
        u.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        u.Gender = gender;
        u.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePatientProfileAsync(Guid userId, PatientProfileInput i)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (u == null) return false;
        if (!string.IsNullOrWhiteSpace(i.FullName)) u.FullName = i.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(i.Phone))    u.Phone    = i.Phone.Trim();
        u.Email = string.IsNullOrWhiteSpace(i.Email) ? null : i.Email.Trim();
        u.Gender = i.Gender;
        u.Dob = i.Dob;
        u.Cccd             = string.IsNullOrWhiteSpace(i.Cccd) ? null : i.Cccd.Trim();
        u.BhytCard         = string.IsNullOrWhiteSpace(i.BhytCard) ? null : i.BhytCard.Trim();
        u.Address          = string.IsNullOrWhiteSpace(i.Address) ? null : i.Address.Trim();
        u.BloodType        = string.IsNullOrWhiteSpace(i.BloodType) ? null : i.BloodType.Trim();
        u.Allergies        = string.IsNullOrWhiteSpace(i.Allergies) ? null : i.Allergies.Trim();
        u.MedicalHistory   = string.IsNullOrWhiteSpace(i.MedicalHistory) ? null : i.MedicalHistory.Trim();
        u.EmergencyContact = string.IsNullOrWhiteSpace(i.EmergencyContact) ? null : i.EmergencyContact.Trim();
        u.LuUpdated = DateTime.Now;
        u.LuUserId  = userId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMedicalInfoAsync(Guid userId, string? bloodType, string? allergies, string? medicalHistory, Guid updatedBy)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (u == null) return false;
        bool changed = false;
        if (!string.IsNullOrWhiteSpace(bloodType) && bloodType.Trim() != (u.BloodType ?? ""))
        {
            u.BloodType = bloodType.Trim();
            changed = true;
        }
        if (!string.IsNullOrWhiteSpace(allergies) && allergies.Trim() != (u.Allergies ?? ""))
        {
            var capped = allergies.Trim();
            if (capped.Length > 500) capped = capped.Substring(0, 500);
            u.Allergies = capped;
            changed = true;
        }
        if (!string.IsNullOrWhiteSpace(medicalHistory) && medicalHistory.Trim() != (u.MedicalHistory ?? ""))
        {
            // medical_history là NVARCHAR(MAX) nên không cap; vẫn trim phòng spam khoảng trắng
            u.MedicalHistory = medicalHistory.Trim();
            changed = true;
        }
        if (changed)
        {
            u.LuUpdated = DateTime.Now;
            u.LuUserId  = updatedBy;
            await _db.SaveChangesAsync();
        }
        return changed;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (u == null) return false;
        if (!StringHelper.VerifyPassword(currentPassword, u.Password)) return false;
        u.Password = StringHelper.HashPassword(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }
}
