using Core.DB.Plugin.Shared.Attributes;
using System.Reflection;

namespace Core.DB.Plugin.Shared.Utils
{
    public static class TypeExtensions
    {
        public static List<PropertyInfo> GetFilteredProperties(this Type t)
        {
            var properties = t.GetProperties().Where(x => !(x.CustomAttributes?.Any(a => a.AttributeType == typeof(CORE_DB_SQL_Ignore)) == true)).ToList();

            return properties;
        }

        public static List<PropertyInfo> GetFilteredProperties_ForAutoIncrementSave(this Type t)
        {
            var properties = t.GetProperties().Where(x => !(x.CustomAttributes?.Any(a => a.AttributeType == typeof(CORE_DB_SQL_PrimaryKey) || a.AttributeType == typeof(CORE_DB_SQL_Ignore)) == true)).ToList();

            return properties;
        }
    }
}