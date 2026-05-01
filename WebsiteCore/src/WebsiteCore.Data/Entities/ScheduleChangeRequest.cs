using System;

namespace WebsiteCore.Data.Entities;

public partial class ScheduleChangeRequest
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateOnly? RequestedDate { get; set; }
    public string? RequestedSession { get; set; }
    public string RequestType { get; set; } = "change";
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = "pending";
    public string? AdminResponse { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
}
