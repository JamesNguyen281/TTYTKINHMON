using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Prescription
{
    public Guid Id { get; set; }

    public Guid MedicalRecordId { get; set; }

    public string DrugName { get; set; } = null!;

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public string? Duration { get; set; }

    public string? Note { get; set; }

    public int? Ord { get; set; }

    public DateTime CreatedDate { get; set; }
}
