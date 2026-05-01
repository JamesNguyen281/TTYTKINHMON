using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string? FullName { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? ImagePath { get; set; }

    public int? Gender { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }

    public int? ActiveFlag { get; set; }

    public string? GroupId { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Cccd { get; set; }

    public string? BhytCard { get; set; }

    public string? Phone { get; set; }

    public string? BloodType { get; set; }

    public string? Allergies { get; set; }

    /// <summary>Tiền sử bệnh / bệnh nền — bác sĩ cập nhật khi khám, BN xem được trên /ho-so.</summary>
    public string? MedicalHistory { get; set; }

    public string? EmergencyContact { get; set; }

    public Guid? DoctorId { get; set; }

    public int FailedAttempts { get; set; }

    public DateTime? LockoutUntil { get; set; }

    public DateTime? LastLogin { get; set; }
}
