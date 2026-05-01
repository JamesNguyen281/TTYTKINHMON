using System;
using System.Collections.Generic;

namespace WebsiteCore.Data.Entities;

public partial class Credential
{
    public string UserGroupId { get; set; } = null!;

    public string RoleId { get; set; } = null!;
}
