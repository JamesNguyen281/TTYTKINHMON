using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Appointment
{
    public Guid Id { get; set; }

    public Guid? PatientUserId { get; set; }

    public string? PatientName { get; set; }

    public string? PatientPhone { get; set; }

    public string? PatientEmail { get; set; }

    public string? DepartmentName { get; set; }

    public Guid? DoctorId { get; set; }

    public DateOnly? AppointmentDate { get; set; }

    public TimeOnly? AppointmentTime { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public string? StaffNote { get; set; }

    public Guid? SiteId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }

    public string? Session { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? BookingCode { get; set; }

    public bool CheckedIn { get; set; }

    /// <summary>
    /// FK tới ClinicRoom — phòng khám trong khoa "Khoa Khám bệnh" mà BN được lễ tân route vào
    /// theo triệu chứng. Nullable: BN cấp cứu hoặc đặt lịch online chưa được phân phòng.
    /// </summary>
    public Guid? ClinicRoomId { get; set; }

    /// <summary>
    /// Cờ cấp cứu: true = BN đến cấp cứu (bypass workflow phòng khám thường), đi thẳng vào
    /// khoa Cấp cứu hoặc chuyên khoa nội trú. BS đang trực ScheduleType="emergency" sẽ tiếp nhận.
    /// </summary>
    public bool IsEmergency { get; set; }
}
