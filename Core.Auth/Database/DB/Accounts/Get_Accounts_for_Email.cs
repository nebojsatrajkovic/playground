using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Core.Auth.Database.DB.Accounts
{
    public class Get_Accounts_for_Email
    {
        public static async Task<List<GAfE>> InvokeAsync(DbConnection connection, DbTransaction transaction, P_GAfE parameter)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            const string commandLocation = "Core.Auth.Database.DB.Accounts.SQL.Get_Accounts_for_Email.sql";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation) ?? throw new InvalidOperationException($"SQL resource not found: {commandLocation}");
            using var streamReader = new StreamReader(stream);
            command.CommandText = await streamReader.ReadToEndAsync();
            command.CommandTimeout = 60;
            var _Email = command.CreateParameter();
            _Email.ParameterName = "@Email";
            _Email.Value = parameter.Email;
            command.Parameters.Add(_Email);
            var _TenantID = command.CreateParameter();
            _TenantID.ParameterName = "@TenantID";
            _TenantID.Value = parameter.TenantID;
            command.Parameters.Add(_TenantID);
            var results = new List<GAfE_Raw>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var resultItem = new GAfE_Raw();
                resultItem.auth_account_id = reader.IsDBNull("auth_account_id") ? default : reader.GetInt32("auth_account_id");
                results.Add(resultItem);
            }

            var result = GAfE_Raw.Convert(results).ToList();
            return result;
        }

        private sealed class GAfE_Raw
        {
            public int auth_account_id { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GAfE> Convert(List<GAfE_Raw> rawResult)
            {
                var groupResult =
                    from el_GAfE in rawResult.Where(element => !EqualsDefaultValue(element.auth_account_id))group el_GAfE by new
                    {
                        el_GAfE.auth_account_id
                    }

                        into gfunct_GAfE
                        let gfunct_GAfE_first = gfunct_GAfE.First()select new GAfE
                        {
                            auth_account_id = gfunct_GAfE.Key.auth_account_id,
                        };
                return groupResult.ToList();
            }
        }
    }

    public class P_GAfE
    {
        public string Email { get; set; }
        public int TenantID { get; set; }
    }

    public class GAfE
    {
        public int auth_account_id { get; set; }
    }
}