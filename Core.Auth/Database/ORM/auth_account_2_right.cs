using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Auth.Database.ORM;

public static class auth_account_2_right
{
    public static DBTable<Model, Query> DB { get; }

    static auth_account_2_right()
    {
        DB = new DBTable<Model, Query>();
    }

    public class Model
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_account_2_right_id { get; set; }
        public int? account_refid { get; set; }
        public int? right_refid { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public int? tenant_id { get; set; }
    }

    public class Query
    {
        public int? auth_account_2_right_id { get; set; } = null;
        public int? account_refid { get; set; } = null;
        public int? right_refid { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public int? tenant_id { get; set; } = null;
    }
}