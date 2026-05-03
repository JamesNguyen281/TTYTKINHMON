using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class DoctorSchedule
{
    public Guid Id { get; set; }

    public Guid DoctorId { get; set; }

    public Guid? DepartmentId { get; set; }

    public byte Weekday { get; set; }

    public string Session { get; set; } = null!;

    public string? Room { get; set; }

    public int? MaxPatients { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidTo { get; set; }

    public string? Note { get; set; }

    public int ActiveFlag { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Loại lịch trực: "clinic" = trực khám chữa bệnh tại phòng khám,
    /// "emergency" = trực cấp cứu, "management" = ban giám đốc xử lí công việc (không khám).
    /// Default null → coi là "clinic" (backward compat với data cũ).
    /// </summary>
    public string? ScheduleType { get; set; }

    /// <summary>
    /// FK tới ClinicRoom — phòng khám mà BS được luân phiên gán vào (chỉ áp dụng khi
    /// ScheduleType = "clinic"). Nullable: BS trực cấp cứu hoặc ban giám đốc không gán phòng.
    /// </summary>
    public Guid? ClinicRoomId { get; set; }
}
