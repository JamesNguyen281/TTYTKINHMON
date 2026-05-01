using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Slide
{
    public Guid Id { get; set; }

    public string? Type { get; set; }

    public string? TitleL { get; set; }

    public string? TitleE { get; set; }

    public string? DescriptionL { get; set; }

    public string? DescriptionE { get; set; }

    public string? ImagePath { get; set; }

    public string? Icon { get; set; }

    public string? Link { get; set; }

    public string? CssClass { get; set; }

    public int? Ord { get; set; }

    public int? ActiveFlag { get; set; }

    public Guid? SiteId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
