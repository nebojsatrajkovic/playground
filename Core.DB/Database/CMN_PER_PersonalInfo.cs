using System;
using System.Collections.Generic;

namespace Core.DB.Database;

public partial class CMN_PER_PersonalInfo
{
    public int CMN_PER_PersonalInfo1 { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int USR_User_RefID { get; set; }

    public virtual USR_Users USR_User_Ref { get; set; } = null!;
}
