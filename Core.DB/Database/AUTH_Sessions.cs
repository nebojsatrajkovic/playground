using Core.Shared;

namespace Core.DB.Database;

public class AUTH_Sessions
{
    public class Model
    {
        public Guid AUTH_SessionID { get; set; }

        public string SessionToken { get; set; } = null!;

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public Guid Account_RefID { get; set; }
    }

    public class Query
    {
        public Guid? AUTH_SessionID { get; set; } = null;
        public string? SessionToken { get; set; } = null;
        public DateTime? ValidFrom { get; set; } = null;
        public DateTime? ValidTo { get; set; } = null;
        public Guid? Account_RefID { get; set; } = null;
    }

    public class DB : ADBTable<Model, Query>
    {

    }
}