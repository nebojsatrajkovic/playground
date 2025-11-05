using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.Auth.Database.DB.Accounts
{
    public class Get_Account_Rights
    {
        public static async Task<List<GAR>> InvokeAsync(DbConnection connection, DbTransaction transaction, P_GAR parameter)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            const string commandLocation = "Core.Auth.Database.DB.Accounts.SQL.Get_Account_Rights.sql";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation) ?? throw new InvalidOperationException($"SQL resource not found: {commandLocation}");
            using var streamReader = new StreamReader(stream);
            command.CommandText = await streamReader.ReadToEndAsync();
            command.CommandTimeout = 60;
            var _AccountID = command.CreateParameter();
            _AccountID.ParameterName = "@AccountID";
            _AccountID.Value = parameter.AccountID;
            command.Parameters.Add(_AccountID);
            var _TenantID = command.CreateParameter();
            _TenantID.ParameterName = "@TenantID";
            _TenantID.Value = parameter.TenantID;
            command.Parameters.Add(_TenantID);
            var results = new List<GAR_Raw>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var resultItem = new GAR_Raw();
                resultItem.auth_right_id = reader.IsDBNull("auth_right_id") ? default : reader.GetInt32("auth_right_id");
                resultItem.right_code = reader.IsDBNull("right_code") ? default : reader.GetString("right_code");
                resultItem.right_gid = reader.IsDBNull("right_gid") ? default : reader.GetGuid("right_gid");
                results.Add(resultItem);
            }

            var result = GAR_Raw.Convert(results).ToList();
            return result;
        }

        private sealed class GAR_Raw
        {
            public int auth_right_id { get; set; }
            public string right_code { get; set; }
            public Guid right_gid { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GAR> Convert(List<GAR_Raw> rawResult)
            {
                var groupResult =
                    from el_GAR in rawResult.Where(element => !EqualsDefaultValue(element.auth_right_id))group el_GAR by new
                    {
                        el_GAR.auth_right_id
                    }

                        into gfunct_GAR
                        let gfunct_GAR_first = gfunct_GAR.First()select new GAR
                        {
                            auth_right_id = gfunct_GAR.Key.auth_right_id,
                            right_code = gfunct_GAR_first.right_code,
                            right_gid = gfunct_GAR_first.right_gid,
                        };
                return groupResult.ToList();
            }
        }
    }

    public class P_GAR
    {
        public int AccountID { get; set; }
        public int TenantID { get; set; }
    }

    public class GAR
    {
        public int auth_right_id { get; set; }
        public string right_code { get; set; }
        public Guid right_gid { get; set; }
    }
}