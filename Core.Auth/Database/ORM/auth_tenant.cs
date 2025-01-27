using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Auth.Database.ORM;

public static class auth_tenant
{
    public static DBTable<ORM, QueryParameter> Database { get; }

    static auth_tenant()
    {
        Database = new DBTable<ORM, QueryParameter>();
    }

    public class ORM
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_tenant_id { get; set; }
        public string? tenant_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public bool is_deleted { get; set; }

    }

    public class QueryParameter
    {
        public int? auth_tenant_id { get; set; } = null;
        public string? tenant_name { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public bool? is_deleted { get; set; } = null;

    }
}