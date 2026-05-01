using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Answer
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public Guid DoctorUserId { get; set; }

    public string Body { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Question Question { get; set; } = null!;
}
