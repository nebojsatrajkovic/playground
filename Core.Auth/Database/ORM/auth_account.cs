using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Auth.Database.ORM;

public static class auth_account
{
    public static DBTable<ORM, QueryParameter> Database { get; }

    static auth_account()
    {
        Database = new DBTable<ORM, QueryParameter>();
    }

    public class ORM
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_account_id { get; set; }
        public string email { get; set; } = null!;
        public string username { get; set; } = null!;
        public string password_hash { get; set; } = null!;
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public int tenant_id { get; set; }
        public bool is_verified { get; set; }
        public bool is_disabled { get; set; }
        public bool is_main_account_for_tenant { get; set; }

    }

    public class QueryParameter
    {
        public int? auth_account_id { get; set; } = null;
        public string? email { get; set; } = null;
        public string? username { get; set; } = null;
        public string? password_hash { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public int? tenant_id { get; set; } = null;
        public bool? is_verified { get; set; } = null;
        public bool? is_disabled { get; set; } = null;
        public bool? is_main_account_for_tenant { get; set; } = null;

    }
}