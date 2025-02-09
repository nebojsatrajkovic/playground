using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Cloud.Database.Query.Folder
{
    public class Get_ExistingFolders_for_Name_and_ParentFolder
    {
        public static List<GEFfNaPF> Invoke(DbConnection connection, DbTransaction transaction, P_GEFfNaPF parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            const string commandLocation = "Core.Cloud.Database.Query.Folder.SQL.Get_ExistingFolders_for_Name_and_ParentFolder.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;
            var _ParentFolderID = command.CreateParameter();
            _ParentFolderID.ParameterName = "@ParentFolderID";
            _ParentFolderID.Value = parameter.ParentFolderID;
            command.Parameters.Add(_ParentFolderID);
            var _FolderName = command.CreateParameter();
            _FolderName.ParameterName = "@FolderName";
            _FolderName.Value = parameter.FolderName;
            command.Parameters.Add(_FolderName);
            var _AccountID = command.CreateParameter();
            _AccountID.ParameterName = "@AccountID";
            _AccountID.Value = parameter.AccountID;
            command.Parameters.Add(_AccountID);
            var _TenantID = command.CreateParameter();
            _TenantID.ParameterName = "@TenantID";
            _TenantID.Value = parameter.TenantID;
            command.Parameters.Add(_TenantID);
            var results = new List<GEFfNaPF_Raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new GEFfNaPF_Raw();
                    resultItem.cloud_folder_id = reader.IsDBNull("cloud_folder_id") ? default : reader.GetInt32("cloud_folder_id");
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

            var result = GEFfNaPF_Raw.Convert(results).ToList();
            return result;
        }

        class GEFfNaPF_Raw
        {
            public int cloud_folder_id { get; set; }
            public string session_token { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            internal static List<GEFfNaPF> Convert(List<GEFfNaPF_Raw> rawResult)
            {
                var groupResult =
                    from el_GEFfNaPF in rawResult.Where(element => !EqualsDefaultValue(element.cloud_folder_id)).ToList()group el_GEFfNaPF by new
                    {
                        el_GEFfNaPF.cloud_folder_id
                    }

                        into gfunct_GEFfNaPF
                        select new GEFfNaPF
                        {
                            cloud_folder_id = gfunct_GEFfNaPF.Key.cloud_folder_id,
                            session_token = gfunct_GEFfNaPF.First().session_token,
                        };
                return groupResult.ToList();
            }
        }
    }

    public class P_GEFfNaPF
    {
        public int? ParentFolderID { get; set; }
        public string FolderName { get; set; }
        public int AccountID { get; set; }
        public int TenantID { get; set; }
    }

    public class GEFfNaPF
    {
        public int cloud_folder_id { get; set; }
        public string session_token { get; set; }
    }
}