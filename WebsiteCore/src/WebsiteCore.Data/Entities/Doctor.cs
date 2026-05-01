using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Doctor
{
    public Guid Id { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? NameL { get; set; }

    public string? NameE { get; set; }

    public string? SpeciallyL { get; set; }

    public string? SpeciallyE { get; set; }

    public string? LanguageSpoken { get; set; }

    public string? QuantificationL { get; set; }

    public string? QuantificationE { get; set; }

    public string? ExperiencesL { get; set; }

    public string? ExperiencesE { get; set; }

    public string? SpeciallyInterestsL { get; set; }

    public string? SpeciallyInterestsE { get; set; }

    public string? ImagePath { get; set; }

    public int? Gender { get; set; }

    public string? TimetableL { get; set; }

    public string? TimetableE { get; set; }

    public int? ActiveFlag { get; set; }

    public bool? ShowOnHome { get; set; }

    public int? Ord { get; set; }

    public bool? IsPartner { get; set; }

    public string? Position { get; set; }

    public DateTime? CreatedDate { get; set; }
}
