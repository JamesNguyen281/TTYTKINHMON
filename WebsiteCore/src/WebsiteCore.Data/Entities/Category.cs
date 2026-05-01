using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Category
{
    public Guid Id { get; set; }

    public string? NameL { get; set; }

    public string? NameE { get; set; }

    public string? AliasE { get; set; }

    public string? AliasL { get; set; }

    public string? ImagePath { get; set; }

    public string? DescriptionL { get; set; }

    public string? DescriptionE { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? MenuId { get; set; }

    public int? Ord { get; set; }

    public Guid? CreatedByUser { get; set; }

    public Guid? LuUserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LuUpdated { get; set; }

    public int? ActiveFlag { get; set; }

    public bool? ShowOnHome { get; set; }

    public bool? HotCategory { get; set; }

    public string? Link { get; set; }

    public string? Type { get; set; }

    public string? ThemeType { get; set; }

    public Guid? SiteId { get; set; }

    public int? Level { get; set; }

    public virtual ICollection<News> News { get; set; } = new List<News>();
}
