using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Department
{
    public Guid Id { get; set; }

    public Guid? SiteId { get; set; }

    public string? NameL { get; set; }

    public string? DescriptionL { get; set; }

    public string? DetailL { get; set; }

    public string? ImagePath { get; set; }

    public int? ActiveFlag { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime? LuUpdated { get; set; }

    public Guid? LuUserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? NameE { get; set; }

    public string? DescriptionE { get; set; }

    public string? DetailE { get; set; }

    public int? Ord { get; set; }

    public string? Link { get; set; }

    public string? Alias { get; set; }

    public string? BackgroundImage { get; set; }

    public string? SubLink { get; set; }
}
