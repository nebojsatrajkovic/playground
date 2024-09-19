namespace Core.Shared.Configuration
{
    public static class CORE_Configuration
    {
        public static CORE_API API { get; set; } = null!;
        public static CORE_Database Database { get; set; } = null!;
    }

    public class CORE_API
    {
        public string AuthKey { get; set; } = string.Empty;
    }

    public class CORE_Database
    {
        public string ConnectionString { get; set; } = null!;
    }
}