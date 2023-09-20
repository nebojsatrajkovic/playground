using Core.Shared;

namespace Core.DB.Database;

public static class AUTH_Accounts
{
    public class Model
    {
        public Guid AUTH_AccountID { get; set; }

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }

    public class Query
    {
        public Guid? AUTH_AccountID { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Password { get; set; } = null;
    }

    public class DB : ADBTable<Model, Query>
    {

    }
}