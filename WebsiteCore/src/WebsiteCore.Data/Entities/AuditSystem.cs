using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class AuditSystem
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? ActionDate { get; set; }

    public string? ActionDescription { get; set; }

    public string? ActionDetail { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }

    public int? ActiveFlag { get; set; }
}
