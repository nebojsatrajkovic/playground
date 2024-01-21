using Core.DB.Plugin.Shared.Attributes;
using Core.DB.Plugin.MSSQL.Database;

namespace Core.DB.Database.Tables;

public static class AUTH_Sessions
{
    public class Model
    {
        [CORE_DB_SQL_PrimaryKey]
        public Guid AUTH_SessionID { get; set; } = Guid.NewGuid();
        public string ValidFrom { get; set; } = null!;
        public string ValidTo { get; set; } = null!;
        public string SessionToken { get; set; } = null!;
        public bool IsDeleted { get; set; }

    }

    public class Query
    {
        public Guid? AUTH_SessionID { get; set; } = null;
        public string? ValidFrom { get; set; } = null;
        public string? ValidTo { get; set; } = null;
        public string? SessionToken { get; set; } = null;
        public bool? IsDeleted { get; set; } = null;

    }

    public class DB : ADBTable<Model, Query>
    {

    }
}