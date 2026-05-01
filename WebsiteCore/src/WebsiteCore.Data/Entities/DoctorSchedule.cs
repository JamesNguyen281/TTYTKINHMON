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
}
