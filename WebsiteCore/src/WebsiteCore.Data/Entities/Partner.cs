using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Partner
{
    public Guid Id { get; set; }

    public string? NameL { get; set; }

    public string? NameE { get; set; }

    public string? Link { get; set; }

    public string? ImagePath { get; set; }

    public int? Ord { get; set; }

    public int? ActiveFlag { get; set; }

    public Guid? SiteId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
