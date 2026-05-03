using System;

namespace WebsiteCore.Data.Entities;

/// <summary>
/// Phòng khám trong khoa "Khoa Khám bệnh" — mỗi phòng tương ứng một chuyên khoa
/// (Tim mạch, Tiêu hóa, Nhi, Sản, Hô hấp, …).
///
/// Workflow: bệnh nhân tới quầy lễ tân → trình bày triệu chứng → lễ tân route
/// vào ClinicRoom phù hợp → BS được luân phiên gán vào room qua DoctorSchedule.ClinicRoomId.
///
/// Mỗi room thuộc 1 Department (gateway "Khoa Khám bệnh"). Quan hệ:
///   Department(Khám bệnh) 1 — ∞ ClinicRoom 1 — ∞ DoctorSchedule(luân phiên BS)
///                                              1 — ∞ Appointment(BN được phân vào)
/// </summary>
public partial class ClinicRoom
{
    public Guid Id { get; set; }

    /// <summary>FK tới Department (khoa "Khoa Khám bệnh"). Cùng siteId qua join Department.</summary>
    public Guid DepartmentId { get; set; }

    /// <summary>Mã phòng nội bộ — vd "P101", "P201". Unique trong Department.</summary>
    public string RoomCode { get; set; } = null!;

    /// <summary>Tên phòng — vd "Phòng khám Tim mạch", "Phòng khám Nhi".</summary>
    public string RoomName { get; set; } = null!;

    /// <summary>Chuyên khoa của phòng (tiếng Việt). Hiển thị cho lễ tân + BN.</summary>
    public string? SpecialtyL { get; set; }

    /// <summary>Chuyên khoa (tiếng Anh) — bilingual support.</summary>
    public string? SpecialtyE { get; set; }

    /// <summary>Tầng — vd "Tầng 1", "Tầng 2 khu A". Giúp lễ tân chỉ đường BN.</summary>
    public string? Floor { get; set; }

    /// <summary>Triệu chứng phổ biến mà phòng này tiếp nhận (text, ngắn) — gợi ý lễ tân khi route.</summary>
    public string? CommonSymptoms { get; set; }

    public int? Ord { get; set; }

    public int ActiveFlag { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }
}
