using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Comment
{
    public Guid Id { get; set; }

    public Guid? NewId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ActiveFlag { get; set; }
}
