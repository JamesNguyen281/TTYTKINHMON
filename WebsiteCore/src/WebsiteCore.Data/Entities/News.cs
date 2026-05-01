using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class News
{
    public Guid Id { get; set; }

    public string? TitleL { get; set; }

    public string? TitleE { get; set; }

    public string? AliasL { get; set; }

    public string? AliasE { get; set; }

    public string? ImagePath { get; set; }

    public string? DetailL { get; set; }

    public string? DetailE { get; set; }

    public string? DescriptionL { get; set; }

    public string? DescriptionE { get; set; }

    public Guid? CategoryId { get; set; }

    public int? Ord { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? LuUserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LuUpdated { get; set; }

    public int? ActiveFlag { get; set; }

    public bool? HotNew { get; set; }

    public bool? ShowOnHome { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeyword { get; set; }

    public int? Views { get; set; }

    public string? Copyright { get; set; }

    public Guid? SiteId { get; set; }

    public string? Type { get; set; }

    public string? Link { get; set; }

    public Guid? DepartmentId { get; set; }

    public virtual Category? Category { get; set; }
}
