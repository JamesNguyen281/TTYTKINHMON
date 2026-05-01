using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Question
{
    public Guid Id { get; set; }

    public Guid PatientUserId { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string? Topic { get; set; }

    public bool IsPublic { get; set; }

    public string Status { get; set; } = null!;

    public Guid? SiteId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LuUpdated { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
