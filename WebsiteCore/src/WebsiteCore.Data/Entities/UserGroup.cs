using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class UserGroup
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }
}
