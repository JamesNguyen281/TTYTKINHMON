using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Video
{
    public Guid VideoId { get; set; }

    public string? VideoTitleL { get; set; }

    public string? VideoDescriptionL { get; set; }

    public string? VideoTitleE { get; set; }

    public string? VideoDescriptionE { get; set; }

    public string? VideoThumbnail { get; set; }

    public string? VideoLink { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? CreatedByUser { get; set; }

    public int? Ord { get; set; }

    public int? Status { get; set; }

    public Guid? SiteId { get; set; }
}
