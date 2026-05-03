namespace WebsiteCore.Business.ViewModels;

/// <summary>
/// Thông tin trạng thái slot khám của một bác sĩ tại một ngày + ca cụ thể.
/// Dùng cho lễ tân khi phân BS — chỉ liệt kê BS có lịch trực + còn slot.
/// Đồng thời làm payload cho AJAX dropdown ở /le-tan/lich-hen.
///
/// Logic phân bổ đều: caller sort theo BookedSlots ASC trước khi hiển thị,
/// để BS đang rảnh nhất luôn được đề xuất đầu tiên. Khi lễ tân muốn auto
/// chọn (nút "Phân BS rảnh nhất"), pick item đầu tiên có IsAvailable.
/// </summary>
public class DoctorAvailabilityVm
{
    public Guid DoctorId       { get; set; }
    public string DoctorName   { get; set; } = "";
    public string? Position    { get; set; }   // VD: "BS Chuyên khoa II", "BS Chính" — giúp lễ tân biết trình độ
    public string? Specialty   { get; set; }   // Chuyên môn / sở trường — VD: "Tim mạch", "Tiêu hóa"
    public string? ImagePath   { get; set; }   // Ảnh BS — dùng cho avatar trong dropdown
    public int Ord             { get; set; }   // Thứ tự gốc (giúp tie-break ổn định)

    public Guid DepartmentId   { get; set; }
    public string DepartmentName { get; set; } = "";
    public string? Room        { get; set; }
    public DateOnly Date       { get; set; }
    public string Session      { get; set; } = "";

    /// <summary>Tổng slot tối đa cho BS này tại ca này (lấy từ AppointmentQuota override hoặc DoctorSchedule.MaxPatients).</summary>
    public int MaxSlots        { get; set; }
    /// <summary>Số ca đã được confirm (trừ pending/cancelled/rejected).</summary>
    public int BookedSlots     { get; set; }
    /// <summary>Còn lại = Max − Booked (clamp ≥ 0).</summary>
    public int RemainingSlots  => Math.Max(0, MaxSlots - BookedSlots);
    /// <summary>Tỷ lệ lấp đầy (0–100), dùng để vẽ progress bar cho UI.</summary>
    public int FillPercent     => MaxSlots > 0 ? Math.Min(100, BookedSlots * 100 / MaxSlots) : 0;
    /// <summary>Còn slot (RemainingSlots > 0)? Cho phép pick.</summary>
    public bool IsAvailable    => RemainingSlots > 0;
}

/// <summary>
/// Tổng hợp slot toàn khoa cho 1 ngày + ca: dept-quota tổng + chi tiết từng BS.
/// Lễ tân nhìn vào đây để biết khoa còn bao nhiêu slot tổng, và phân bổ giữa các BS.
/// </summary>
public class DepartmentSlotOverviewVm
{
    public Guid DepartmentId   { get; set; }
    public string DepartmentName { get; set; } = "";
    public DateOnly Date       { get; set; }
    public string Session      { get; set; } = "";

    /// <summary>Quota tổng của khoa (mặc định 40/ca theo Constants.DefaultQuotaPerSession).</summary>
    public int DeptMaxSlots    { get; set; }
    /// <summary>Số ca đã confirm trong khoa (mọi BS + appointment chưa phân BS).</summary>
    public int DeptBookedSlots { get; set; }
    public int DeptRemaining   => Math.Max(0, DeptMaxSlots - DeptBookedSlots);

    public List<DoctorAvailabilityVm> Doctors { get; set; } = new();
}

/// <summary>
/// Nhóm BS theo khoa cho UI "Bác sĩ trực hôm nay" — strongly-typed thay vì dùng anonymous type
/// (Razor không resolve được dynamic key từ anonymous type qua reflection).
/// </summary>
public class DeptDoctorGroup
{
    public Guid   DepartmentId   { get; set; }
    public string DepartmentName { get; set; } = "";
    public List<DoctorAvailabilityVm> Doctors { get; set; } = new();
}

