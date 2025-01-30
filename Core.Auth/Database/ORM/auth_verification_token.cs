using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Auth.Database.ORM;

public static class auth_verification_token
{
    public static DBTable<ORM, QueryParameter> Database { get; }

    static auth_verification_token()
    {
        Database = new DBTable<ORM, QueryParameter>();
    }

    public class ORM
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_verification_token_id { get; set; }
        public int account_refid { get; set; }
        public bool is_forgot_password { get; set; }
        public bool is_account_verification { get; set; }
        public string? token { get; set; }
        public DateTime? expires_at { get; set; }
        public bool is_processed { get; set; }
        public bool is_confirmed { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public bool is_deleted { get; set; }
        public int tenant_id { get; set; }

    }

    public class QueryParameter
    {
        public int? auth_verification_token_id { get; set; } = null;
        public int? account_refid { get; set; } = null;
        public bool? is_forgot_password { get; set; } = null;
        public bool? is_account_verification { get; set; } = null;
        public string? token { get; set; } = null;
        public DateTime? expires_at { get; set; } = null;
        public bool? is_processed { get; set; } = null;
        public bool? is_confirmed { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public int? tenant_id { get; set; } = null;

    }
}