namespace WebsiteCore.Business;

/// <summary>
/// Hằng số dùng chung trong toàn project.
/// Group ID dùng làm role marker trong cột User.group_id.
/// </summary>
public static class Constants
{
    // User group_id (cột User.group_id)
    public const string AdminGroup     = "ADMIN";
    public const string PosterGroup    = "POSTER";
    public const string DoctorGroup    = "DOCTOR";
    public const string ReceptionGroup = "RECEPTION";
    public const string MemberGroup    = "MEMBER";

    // Session keys
    public const string UserSession       = "USER";
    public const string CredentialSession = "CREDENTIAL";
    public const string SiteSession       = "Site";
    public const string LocateClient      = "locate_client";
    public const string ForcePwdChange    = "FORCE_PWD_CHANGE";

    // Category type values
    public const string TypeMainMenu          = "MAIN_MENU";
    public const string TypeBlockFooter       = "BLOCK_FOOTER";
    public const string TypeBlockSlide        = "BLOCK_SLIDE";
    public const string TypeBlockSlideText    = "BLOCK_SLIDE_TEXT";
    public const string TypeOutstandingService = "OUTSTANDING_SERVICE";

    // Appointment statuses (cột Appointment.status — string trong DB)
    public const string ApptPending     = "pending";
    public const string ApptConfirmed   = "confirmed";
    public const string ApptRejected    = "rejected";
    public const string ApptRescheduled = "rescheduled";
    public const string ApptCompleted   = "completed";
    public const string ApptCancelled   = "cancelled";

    // Sessions trong ngày
    public const string SessionMorning   = "morning";
    public const string SessionAfternoon = "afternoon";

    // Quota mặc định mỗi buổi (dùng khi insert AppointmentQuota mới)
    public const int DefaultQuotaPerSession = 30;

    // Max ngày đặt lịch trước (block lịch quá xa)
    public const int MaxDaysAhead = 30;

    /// <summary>Label tiếng Việt cho trạng thái lịch khám — dùng đồng bộ ở list/detail/portal.</summary>
    public static string ApptStatusLabel(string? status) => status switch
    {
        ApptPending     => "Chờ duyệt",
        ApptConfirmed   => "Đã xác nhận",
        ApptRescheduled => "Đề nghị đổi lịch",
        ApptCompleted   => "Hoàn tất",
        ApptRejected    => "Từ chối",
        ApptCancelled   => "Đã huỷ",
        _ => status ?? string.Empty
    };
}
