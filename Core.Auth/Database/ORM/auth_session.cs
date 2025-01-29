using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Auth.Database.ORM;

public static class auth_session
{
    public static DBTable<ORM, QueryParameter> Database { get; }

    static auth_session()
    {
        Database = new DBTable<ORM, QueryParameter>();
    }

    public class ORM
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_session_id { get; set; }
        public int account_refid { get; set; }
        public string session_token { get; set; } = null!;
        public DateTime valid_from { get; set; }
        public DateTime valid_to { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public int tenant_id { get; set; }

    }

    public class QueryParameter
    {
        public int? auth_session_id { get; set; } = null;
        public int? account_refid { get; set; } = null;
        public string? session_token { get; set; } = null;
        public DateTime? valid_from { get; set; } = null;
        public DateTime? valid_to { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public int? tenant_id { get; set; } = null;

    }
}