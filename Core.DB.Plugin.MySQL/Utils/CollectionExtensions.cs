namespace Core.DB.Plugin.MySQL.Utils
{
    internal static class CollectionExtensions
    {
        internal static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        internal static bool HasValue<T>(this IEnumerable<T> collection)
        {
            return !IsEmpty(collection);
        }
    }
}