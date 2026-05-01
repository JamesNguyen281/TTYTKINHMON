using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class AppointmentQuotum
{
    public Guid Id { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? DoctorId { get; set; }

    public DateOnly ApptDate { get; set; }

    public string Session { get; set; } = null!;

    public int MaxCount { get; set; }

    public int BookedCount { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LuUpdated { get; set; }
}
