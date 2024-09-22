using System.Reflection;

namespace Core.DB.Plugin.Shared.Interfaces
{
    public interface IDB_Table
    {
        PropertyInfo? GetPrimaryKeyProperty();
    }
}