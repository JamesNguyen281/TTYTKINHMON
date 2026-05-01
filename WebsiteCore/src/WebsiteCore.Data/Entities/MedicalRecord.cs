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
}
