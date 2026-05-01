using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Page
{
    public Guid Id { get; set; }

    public string TitleL { get; set; } = null!;

    public string? TitleE { get; set; }

    public string? DetailL { get; set; }

    public string? DetailE { get; set; }

    public Guid? MenuId { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeyword { get; set; }

    public int? ActiveFlag { get; set; }

    public DateTime? CreateDate { get; set; }

    public Guid? CreateByUserId { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }
}
