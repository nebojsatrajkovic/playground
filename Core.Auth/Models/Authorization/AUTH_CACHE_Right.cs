namespace Core.Auth.Models.Authorization
{
    internal class AUTH_CACHE_Right
    {
        internal HashSet<string> Rights { get; set; } = null!;
        internal DateTime LastAccessedAt_UTC { get; set; }
    }
}