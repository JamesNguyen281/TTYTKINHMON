namespace WebsiteCore.Business.ViewModels;

/// <summary>
/// Input model cho bệnh nhân tự cập nhật hồ sơ tại /ho-so.
/// Chỉ chứa những field bệnh nhân được phép sửa — không bao gồm UserName, GroupId, ActiveFlag.
/// </summary>
public class PatientProfileInput
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int? Gender { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Cccd { get; set; }
    public string? BhytCard { get; set; }
    public string? Address { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
    public string? EmergencyContact { get; set; }
}
