using Core.DB.Plugin.MySQL.Database;
using Core.DB.Plugin.Shared.Attributes;
using Core.DB.Plugin.Shared.Interfaces;
using System.Reflection;

namespace Core.Auth.Database.ORM;

public static class auth_tenant
{
    public class Model : IDB_Table
    {
        [CORE_DB_SQL_PrimaryKey]
        public int auth_tenant_id { get; set; }
        public string? tenant_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public bool is_deleted { get; set; }
        public int tenant_id { get; set; }

        public PropertyInfo? GetPrimaryKeyProperty()
        {
            return GetType().GetProperty("auth_tenant_id");
        }
    }

    public class Query
    {
        public int? auth_tenant_id { get; set; } = null;
        public string? tenant_name { get; set; } = null;
        public DateTime? created_at { get; set; } = null;
        public DateTime? modified_at { get; set; } = null;
        public bool? is_deleted { get; set; } = null;
        public int? tenant_id { get; set; } = null;

    }

    public class DB : ADBTable<Model, Query>
    {

    }
}