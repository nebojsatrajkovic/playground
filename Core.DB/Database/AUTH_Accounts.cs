using System;
using System.Collections.Generic;

namespace Core.DB.Database;

public partial class AUTH_Accounts
{
    public int AUTH_AccountID { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<AUTH_Sessions> AUTH_Sessions { get; set; } = new List<AUTH_Sessions>();
}
