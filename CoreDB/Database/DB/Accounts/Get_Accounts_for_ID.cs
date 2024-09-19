using System.Data;
using System.Data.Common;
using System.Text;

namespace CoreDB.Database.DB.Accounts
{
    public class Get_Accounts_for_ID
    {
        public static GAfID Invoke(DbConnection connection, DbTransaction transaction, P_GAfID parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            const string commandLocation = "CoreDB.Database.DB.Accounts.SQL.Get_Accounts_for_ID.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;
            var _AccountID = command.CreateParameter();
            _AccountID.ParameterName = "@AccountID";
            _AccountID.Value = parameter.AccountID;
            command.Parameters.Add(_AccountID);
            var results = new List<GAfID_Raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new GAfID_Raw();
                    resultItem.auth_account_id = reader.IsDBNull("auth_account_id") ? default : reader.GetInt32("auth_account_id");
                    resultItem.email = reader.IsDBNull("email") ? default : reader.GetString("email");
                    resultItem.username = reader.IsDBNull("username") ? default : reader.GetString("username");
                    results.Add(resultItem);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                reader.Close();
                throw;
            }

            var result = GAfID_Raw.Convert(results).FirstOrDefault();
            return result;
        }

        class GAfID_Raw
        {
            public int auth_account_id { get; set; }
            public string email { get; set; }
            public string username { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GAfID> Convert(List<GAfID_Raw> rawResult)
            {
                var groupResult =
                    from el_GAfID in rawResult.Where(element => !EqualsDefaultValue(element.auth_account_id)).ToList()
                    group el_GAfID by new
                    {
                        el_GAfID.auth_account_id
                    }
                    into gfunct_GAfID
                    select new GAfID
                    {
                        auth_account_id = gfunct_GAfID.Key.auth_account_id,
                        email = gfunct_GAfID.First().email,
                        username = gfunct_GAfID.First().username,
                    };
                return groupResult.ToList();
            }
        }
    }

    public class P_GAfID
    {
        public int AccountID { get; set; }
    }

    public class GAfID
    {
        public int auth_account_id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
    }
}