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
    public class Get_Tenants_for_AccountEmail
    {
        public static async Task<List<GTfAE>> InvokeAsync(DbConnection connection, DbTransaction transaction, P_GTfAE parameter)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            const string commandLocation = "Core.Auth.Database.DB.Accounts.SQL.Get_Tenants_for_AccountEmail.sql";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation) ?? throw new InvalidOperationException($"SQL resource not found: {commandLocation}");
            using var streamReader = new StreamReader(stream);
            command.CommandText = await streamReader.ReadToEndAsync();
            command.CommandTimeout = 60;
            var _Email = command.CreateParameter();
            _Email.ParameterName = "@Email";
            _Email.Value = parameter.Email;
            command.Parameters.Add(_Email);
            var results = new List<GTfAE_Raw>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var resultItem = new GTfAE_Raw();
                resultItem.auth_tenant_id = reader.IsDBNull("auth_tenant_id") ? default : reader.GetInt32("auth_tenant_id");
                resultItem.tenant_name = reader.IsDBNull("tenant_name") ? default : reader.GetString("tenant_name");
                results.Add(resultItem);
            }

            var result = GTfAE_Raw.Convert(results).ToList();
            return result;
        }

        private sealed class GTfAE_Raw
        {
            public int auth_tenant_id { get; set; }
            public string tenant_name { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GTfAE> Convert(List<GTfAE_Raw> rawResult)
            {
                var groupResult =
                    from el_GTfAE in rawResult.Where(element => !EqualsDefaultValue(element.auth_tenant_id))group el_GTfAE by new
                    {
                        el_GTfAE.auth_tenant_id
                    }

                        into gfunct_GTfAE
                        let gfunct_GTfAE_first = gfunct_GTfAE.First()select new GTfAE
                        {
                            auth_tenant_id = gfunct_GTfAE.Key.auth_tenant_id,
                            tenant_name = gfunct_GTfAE_first.tenant_name,
                        };
                return groupResult.ToList();
            }
        }
    }

    public class P_GTfAE
    {
        public string Email { get; set; }
    }

    public class GTfAE
    {
        public int auth_tenant_id { get; set; }
        public string tenant_name { get; set; }
    }
}