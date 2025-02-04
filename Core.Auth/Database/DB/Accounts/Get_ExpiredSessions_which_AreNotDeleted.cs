using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Auth.Database.DB.Accounts
{
    public class Get_ExpiredSessions_which_AreNotDeleted
    {
        public static List<GESwAND> Invoke(DbConnection connection, DbTransaction transaction, P_GESwAND parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            const string commandLocation = "Core.Auth.Database.DB.Accounts.SQL.Get_ExpiredSessions_which_AreNotDeleted.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;
            var _TenantID = command.CreateParameter();
            _TenantID.ParameterName = "@TenantID";
            _TenantID.Value = parameter.TenantID;
            command.Parameters.Add(_TenantID);
            var _DateThreshold = command.CreateParameter();
            _DateThreshold.ParameterName = "@DateThreshold";
            _DateThreshold.Value = parameter.DateThreshold;
            command.Parameters.Add(_DateThreshold);
            var results = new List<GESwAND_Raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new GESwAND_Raw();
                    resultItem.auth_session_id = reader.IsDBNull("auth_session_id") ? default : reader.GetInt32("auth_session_id");
                    resultItem.session_token = reader.IsDBNull("session_token") ? default : reader.GetString("session_token");
                    results.Add(resultItem);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                reader.Close();
                throw;
            }

            var result = GESwAND_Raw.Convert(results).ToList();
            return result;
        }

        class GESwAND_Raw
        {
            public int auth_session_id { get; set; }
            public string session_token { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GESwAND> Convert(List<GESwAND_Raw> rawResult)
            {
                var groupResult =
                    from el_GESwAND in rawResult.Where(element => !EqualsDefaultValue(element.auth_session_id)).ToList()group el_GESwAND by new
                    {
                        el_GESwAND.auth_session_id
                    }

                        into gfunct_GESwAND
                        select new GESwAND
                        {
                            auth_session_id = gfunct_GESwAND.Key.auth_session_id,
                            session_token = gfunct_GESwAND.First().session_token,
                        };
                return groupResult.ToList();
            }
        }
    }

    public class P_GESwAND
    {
        public int TenantID { get; set; }
        public DateTime DateThreshold { get; set; }
    }

    public class GESwAND
    {
        public int auth_session_id { get; set; }
        public string session_token { get; set; }
    }
}