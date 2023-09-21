using Core.Shared;
using Core.Shared.Attributes;

namespace Core.DB.Database;

public class AUTH_Sessions
{
    public class Model
    {
        [CORE_DB_PrimaryKey]
        public Guid AUTH_SessionID { get; set; }
        public string SessionToken { get; set; } = null!;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public Guid Account_RefID { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class Query
    {
        public Guid? AUTH_SessionID { get; set; } = null;
        public string? SessionToken { get; set; } = null;
        public DateTime? ValidFrom { get; set; } = null;
        public DateTime? ValidTo { get; set; } = null;
        public Guid? Account_RefID { get; set; } = null;
        public bool? IsDeleted { get; set; } = null;
    }

    public class DB : ADBTable<Model, Query>
    {

    }
}