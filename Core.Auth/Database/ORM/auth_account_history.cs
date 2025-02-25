using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Auth.Database.ORM
{
    public static class auth_account_history
    {
        public static DBTable<ORM, QueryParameter> Database { get; }

        static auth_account_history()
        {
            Database = new DBTable<ORM, QueryParameter>();
        }

        public class ORM
        {
            [CORE_DB_SQL_PrimaryKey]
            public int auth_account_history_id { get; set; }
            public int account_refid { get; set; }
            public bool is_account_activated { get; set; }
            public bool is_account_deactivated { get; set; }
            public bool is_account_deleted { get; set; }
            public int performed_by_account_refid { get; set; }


            public bool is_deleted { get; set; }
            public DateTime created_at { get; set; }
            public DateTime modified_at { get; set; }
            public int tenant_refid { get; set; }

        }

        public class QueryParameter
        {
            public int? auth_account_history_id { get; set; } = null;
            public int? account_refid { get; set; } = null;
            public bool? is_account_activated { get; set; } = null;
            public bool? is_account_deactivated { get; set; } = null;
            public bool? is_account_deleted { get; set; } = null;
            public int? performed_by_account_refid { get; set; } = null;


            public bool? is_deleted { get; set; } = null;
            public DateTime? created_at { get; set; } = null;
            public DateTime? modified_at { get; set; } = null;
            public int? tenant_refid { get; set; } = null;

        }
    }
}
