namespace CORE_VS_PLUGIN.MSSQL_GENERATOR
{
    public interface ICORE_DB_GENERATOR_Configuration
    {
        string ORM_Location { get; set; }
        string ORM_Namespace { get; set; }
        string ConnectionString { get; set; }
    }

    public class CORE_DB_GENERATOR_Configuration : ICORE_DB_GENERATOR_Configuration
    {
        public string ORM_Location { get; set; }
        public string ORM_Namespace { get; set; }
        public string ConnectionString { get; set; }
    }
}