using Core.Shared;
using Core.Shared.Attributes;

namespace Core.DB.Database.Tables;

public static class AUTH_Accounts
{
    public class Model
    {
        [CORE_DB_PrimaryKey]
        public Guid AUTH_AccountID { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsDeleted { get; set; }

        [CORE_DB_Ignore]
        [CORE_DB_AlreadySaved]
        public bool IsAlreadySaved { get; set; }
    }

    public class Query
    {
        public Guid? AUTH_AccountID { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Password { get; set; } = null;
        public bool? IsDeleted { get; set; } = null;

    }

    public class DB : ADBTable<Model, Query>
    {

    }
}