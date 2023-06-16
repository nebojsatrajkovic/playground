using System;
using System.Collections.Generic;

namespace Core.DB.Database;

public partial class AUTH_Sessions
{
    public int AUTH_SessionID { get; set; }

    public string SessionToken { get; set; } = null!;

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public int Account_RefID { get; set; }

    public virtual AUTH_Accounts Account_Ref { get; set; } = null!;
}
