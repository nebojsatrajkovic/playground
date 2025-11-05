namespace Core.Auth.Models.Authorization
{
    internal class CachedRight
    {
        internal HashSet<string> Rights { get; set; } = null!;
        internal DateTime LastAccessedAt_UTC { get; set; }
    }
}