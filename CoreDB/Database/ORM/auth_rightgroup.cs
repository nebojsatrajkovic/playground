using Core.DB.Plugin.Shared.Attributes;
using Core.DB.Plugin.MySQL.Database;

namespace CoreDB.Database.ORM;

public static class auth_rightgroup
{
    public class Model
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_rightgroup_id { get; set; }
        public string? rightgroup_name { get; set; }
        public int? parent_refid { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public int? tenant_id { get; set; }

    }

    public class Query
    {
        public int? auth_rightgroup_id { get; set; } = null;
        public string? rightgroup_name { get; set; } = null;
        public int? parent_refid { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public int? tenant_id { get; set; } = null;

    }

    public class DB : ADBTable<Model, Query>
    {

    }
}