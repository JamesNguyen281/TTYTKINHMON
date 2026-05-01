using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Document
{
    public Guid Id { get; set; }

    public Guid? CategoryId { get; set; }

    public int? Type { get; set; }

    public DateTime? EffectiveFromDate { get; set; }

    public DateTime? EffectiveToDate { get; set; }

    public string? DocumentName { get; set; }

    public string? DocumentCode { get; set; }

    public DateTime? DocumentDate { get; set; }

    public string? AttachFilePath { get; set; }

    public string? BinLocation { get; set; }

    public string? Description { get; set; }

    public string? Owner { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? ApprovedBy { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public Guid? LuUserId { get; set; }

    public DateTime? LuUpdated { get; set; }

    public int? ActiveFlag { get; set; }

    public Guid? SiteId { get; set; }
}
