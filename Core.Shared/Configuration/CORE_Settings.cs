namespace Core.Shared.Configuration
{
    public interface ICORE_Configuration
    {
        CORE_Database Database { get; set; }
    }

    public class CORE_Configuration : ICORE_Configuration
    {
        public static string AuthKey { get; set; } = string.Empty;
        public CORE_Database Database { get; set; } = null!;
    }

    public class CORE_Database
    {
        public string ConnectionString { get; set; } = null!;
    }
}