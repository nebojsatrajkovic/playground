using Core.Shared;
using Core.Shared.Attributes;

namespace Core.DB.Database;

public static class AUTH_Accounts
{
    public class Model
    {
        [CORE_DB_PrimaryKey]
        public Guid AUTH_AccountID { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsDeleted { get; set; }
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