using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Site
{
    public Guid Id { get; set; }

    public string? NameCompanyL { get; set; }

    public string? NameCompanyE { get; set; }

    public string? Favicon { get; set; }

    public string? AddressL { get; set; }

    public string? AddressE { get; set; }

    public string? Map { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? MobilePhone { get; set; }

    public string? Hotline { get; set; }

    public string? EmergencyNumber { get; set; }

    public string? TimeOpen { get; set; }

    public int? ActiveFlag { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeyword { get; set; }

    public int? Ord { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LogoUrl { get; set; }

    public string? DashboardImage { get; set; }
}
