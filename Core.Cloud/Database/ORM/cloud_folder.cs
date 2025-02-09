using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;

namespace Core.Cloud.Database.ORM;

public static class cloud_folder
{
    public static DBTable<ORM, QueryParameter> Database { get; }

    static cloud_folder()
    {
        Database = new DBTable<ORM, QueryParameter>();
    }

    public class ORM
    {
        [CORE_DB_SQL_PrimaryKey]
        public int cloud_folder_id { get; set; }
        public string? folder_name { get; set; }
        public int? parent_folder_refid { get; set; }
        public int auth_account_refid { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public int tenant_refid { get; set; }

    }

    public class QueryParameter
    {
        public int? cloud_folder_id { get; set; } = null;
        public string? folder_name { get; set; } = null;
        public int? parent_folder_refid { get; set; } = null;
        public int? auth_account_refid { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public int? tenant_refid { get; set; } = null;

    }
}