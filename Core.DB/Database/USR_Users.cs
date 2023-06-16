using System;
using System.Collections.Generic;

namespace Core.DB.Database;

public partial class USR_Users
{
    public int USR_UserID { get; set; }

    public DateTime CreationDate { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<CMN_PER_PersonalInfo> CMN_PER_PersonalInfo { get; set; } = new List<CMN_PER_PersonalInfo>();
}
