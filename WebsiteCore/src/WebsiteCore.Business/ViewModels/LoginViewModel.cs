using System.ComponentModel.DataAnnotations;

namespace WebsiteCore.Business.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [RegularExpression(@"^[a-z0-9_.]+$", ErrorMessage = "Chỉ chấp nhận chữ thường, số, dấu chấm hoặc gạch dưới")]
    [MinLength(4, ErrorMessage = "Tên đăng nhập tối thiểu 4 ký tự")]
    [MaxLength(50)]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [MaxLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^[0-9+\-\s]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    [MaxLength(100)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$",
        ErrorMessage = "Mật khẩu phải có chữ HOA, chữ thường, số và ký tự đặc biệt")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [DataType(DataType.Password)]
    [Display(Name = "Nhập lại mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>"male" / "female" / "other"</summary>
    [Display(Name = "Giới tính")]
    public string? Gender { get; set; } = "other";
}

/// <summary>Session-stored snapshot of the logged-in user (POCO, JSON-serializable).</summary>
public class LoggedInUser
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? GroupId { get; set; }
    public Guid? DoctorId { get; set; }
}
