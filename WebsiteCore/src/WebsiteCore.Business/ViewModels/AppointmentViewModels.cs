using System.ComponentModel.DataAnnotations;

namespace WebsiteCore.Business.ViewModels;

public class BookingInputModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [MaxLength(150)]
    [Display(Name = "Họ và tên")]
    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^[0-9+\-\s]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string PatientPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    [Display(Name = "Email")]
    public string? PatientEmail { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chuyên khoa")]
    [Display(Name = "Chuyên khoa")]
    public Guid? DepartmentId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày khám")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Vui lòng chọn buổi khám")]
    [Display(Name = "Buổi khám")]
    public string Session { get; set; } = "morning";

    [MaxLength(500)]
    [Display(Name = "Lý do đến khám")]
    public string? Reason { get; set; }
}

public class AppointmentRow
{
    public Guid Id { get; set; }
    public Guid? SiteId { get; set; }
    public string? BookingCode { get; set; }
    public string? PatientName { get; set; }
    public string? PatientPhone { get; set; }
    public string? PatientEmail { get; set; }
    public Guid? PatientUserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DoctorId { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? Session { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public string? StaffNote { get; set; }
    public bool CheckedIn { get; set; }
    public DateTime? CreatedDate { get; set; }

    /// <summary>P2.A — Phòng khám được lễ tân route BN vào (trong khoa Khoa Khám bệnh).</summary>
    public Guid? ClinicRoomId { get; set; }

    /// <summary>P2.A — Cờ cấp cứu — BN bypass workflow phòng khám thường.</summary>
    public bool IsEmergency { get; set; }
}

public class BookingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? AppointmentId { get; set; }
}

/// <summary>P3.B — kết quả hẹn khám lại. Có BookingCode để BN lưu.</summary>
public class ScheduleFollowUpResult
{
    public bool   Success       { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid?  AppointmentId { get; set; }
    public string? BookingCode  { get; set; }
    public DateOnly? FollowUpDate { get; set; }

    public static ScheduleFollowUpResult Fail(string msg) => new() { Success = false, ErrorMessage = msg };
}

public class UpdateStatusResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? BookingCode { get; set; }

    public static UpdateStatusResult Fail(string msg) => new() { Success = false, ErrorMessage = msg };
}
