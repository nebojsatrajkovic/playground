using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Auth.Database.DB.Tenants
{
    public class Get_Tenants_for_Name
    {
        public static List<GTfN> Invoke(DbConnection connection, DbTransaction transaction, P_GTfN parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            const string commandLocation = "Core.Auth.Database.DB.Tenants.SQL.Get_Tenants_for_Name.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;
            var _Name = command.CreateParameter();
            _Name.ParameterName = "@Name";
            _Name.Value = parameter.Name;
            command.Parameters.Add(_Name);
            var results = new List<GTfN_Raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new GTfN_Raw();
                    resultItem.auth_tenant_id = reader.IsDBNull("auth_tenant_id") ? default : reader.GetInt32("auth_tenant_id");
                    resultItem.tenant_name = reader.IsDBNull("tenant_name") ? default : reader.GetString("tenant_name");
                    results.Add(resultItem);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                reader.Close();
                throw;
            }

            var result = GTfN_Raw.Convert(results).ToList();
            return result;
        }

        class GTfN_Raw
        {
            public int auth_tenant_id { get; set; }
            public string tenant_name { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GTfN> Convert(List<GTfN_Raw> rawResult)
            {
                var groupResult =
                    from el_GTfN in rawResult.Where(element => !EqualsDefaultValue(element.auth_tenant_id)).ToList()group el_GTfN by new
                    {
                        el_GTfN.auth_tenant_id
                    }

                        into gfunct_GTfN
                        select new GTfN
                        {
                            auth_tenant_id = gfunct_GTfN.Key.auth_tenant_id,
                            tenant_name = gfunct_GTfN.First().tenant_name,
                        };
                return groupResult.ToList();
            }
        }
    }

    public class P_GTfN
    {
        public string Name { get; set; }
    }

    public class GTfN
    {
        public int auth_tenant_id { get; set; }
        public string tenant_name { get; set; }
    }
}