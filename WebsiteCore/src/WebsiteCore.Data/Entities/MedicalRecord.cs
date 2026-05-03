using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class MedicalRecord
{
    public Guid Id { get; set; }

    public Guid PatientUserId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? DoctorId { get; set; }

    public Guid? DepartmentId { get; set; }

    public DateTime VisitDate { get; set; }

    public string? ChiefComplaint { get; set; }

    public string? Diagnosis { get; set; }

    public string? TreatmentPlan { get; set; }

    public string? Notes { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public string? RecordNo { get; set; }

    public int ActiveFlag { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }

    /// <summary>
    /// Loại hồ sơ: "outpatient" = ngoại trú (kê đơn về), "inpatient" = nội trú (BS làm hồ sơ
    /// bệnh án + điều dưỡng chuyển BN lên khoa nhập viện).
    /// Default null → coi là "outpatient" (backward compat).
    /// </summary>
    public string? RecordType { get; set; }

    /// <summary>
    /// Cờ nhập viện: true = BS chỉ định nằm viện. Khi true thì TargetInpatientDeptId phải có giá trị.
    /// </summary>
    public bool IsHospitalized { get; set; }

    /// <summary>
    /// FK tới Department mà BN sẽ nhập viện (chỉ áp dụng khi IsHospitalized = true).
    /// Khoa nội trú đích — vd Khoa Nội, Khoa Sản, Khoa Nhi…
    /// </summary>
    public Guid? TargetInpatientDeptId { get; set; }

    /// <summary>Lý do nhập viện + ghi chú dặn điều dưỡng (ngắn, ≤ 500 ký tự).</summary>
    public string? HospitalizationNote { get; set; }
}
