using Core.DB.Plugin.Shared.Attributes;
using Core.DB.Plugin.PostgreSQL.Database;

namespace Core.DB.Database.Tables;

public static class USR_Accounts
{
    public class Model
    {
        [CORE_DB_SQL_PrimaryKey]
        public Guid USR_AccountID { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public bool IsDeleted { get; set; }

        [CORE_DB_SQL_Ignore]
        [CORE_DB_SQL_AlreadySaved]
        public bool IsAlreadySaved { get; set; }
    }

    public class Query
    {
        public Guid? USR_AccountID { get; set; } = null;
        public string? Email { get; set; } = null;
        public bool? IsDeleted { get; set; } = null;

    }

    public class DB : ADBTable<Model, Query>
    {

    }
}