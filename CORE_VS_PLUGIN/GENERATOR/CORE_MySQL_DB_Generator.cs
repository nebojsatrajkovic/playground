﻿using CORE_VS_PLUGIN.MSSQL_GENERATOR;
using CORE_VS_PLUGIN.MSSQL_GENERATOR.Enumerations;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CORE_VS_PLUGIN.GENERATOR
{
    public static class CORE_MySQL_DB_Generator
    {
        public static bool GenerateORMs_From_MySQL(string configurationFilePath)
        {
            var isSuccess = false;

            #region load configuration

            var configurationJson = File.ReadAllText(configurationFilePath);

            var configuration = JsonConvert.DeserializeObject<CORE_DB_GENERATOR_Configuration>(configurationJson);

            try { Directory.Delete(configuration.ORM_Location, true); } catch (Exception) { }
            try { Directory.CreateDirectory(configuration.ORM_Location); } catch (Exception) { }

            string template;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CORE_VS_PLUGIN.GENERATOR.Templates.DB_ORM_TEMPLATE.txt"))
            {
                using (var reader = new StreamReader(stream))
                {
                    template = reader.ReadToEnd();
                }
            }

            #endregion load configuration

            using (var connection = new MySqlConnection(configuration.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        #region load table names

                        var query_GetTables = "SHOW TABLES;";
                        var tableNames = new List<string>();

                        using (var command_GetTables = new MySqlCommand(query_GetTables, connection, transaction))
                        {
                            using (var reader_GetTable = command_GetTables.ExecuteReader())
                            {
                                while (reader_GetTable.Read())
                                {
                                    var value = reader_GetTable.GetString(0);

                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        tableNames.Add(value);
                                    }
                                }

                                reader_GetTable.Close();
                            }
                        }

                        if (tableNames == null || !tableNames.Any()) { Console.WriteLine("No tables were found to generate ORMs"); return false; }

                        #endregion load table names

                        #region load tables data

                        var tables = new List<CORE_DB_TABLE>();

                        foreach (var tableName in tableNames)
                        {
                            var query_GetTableData = $"SHOW COLUMNS FROM {tableName};";

                            var table = new CORE_DB_TABLE
                            {
                                Name = tableName
                            };

                            using (var command_GetTableData = new MySqlCommand(query_GetTableData, connection, transaction))
                            {
                                using (var reader_GetTableData = command_GetTableData.ExecuteReader())
                                {
                                    while (reader_GetTableData.Read())
                                    {
                                        table.Columns.Add(new CORE_DB_TABLE_COLUMN
                                        {
                                            Name = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("Field")),
                                            TypeName = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("Type")),
                                            DataType = 0,
                                            IsNullable = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("Null")) != "NO",
                                            OrdinalPosition = 0,
                                            Is_MySQL_PrimaryKey = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("Key")) == "Pri"
                                        });
                                    }

                                    reader_GetTableData.Close();
                                }
                            }

                            if (table.Columns == null || !table.Columns.Any()) { continue; }

                            tables.Add(table);
                        }

                        if (tables == null || !tables.Any()) { Console.WriteLine("No table data was found to generate ORMs"); return false; }

                        #endregion load tables data

                        #region generate orm using template

                        foreach (var table in tables)
                        {
                            if (table.Columns != null && table.Columns.Any())
                            {
                                var modelBuilder = new StringBuilder();
                                var queryBuilder = new StringBuilder();

                                foreach (var column in table.Columns)
                                {
                                    // TODO implement

                                    continue;
                                }

                                var tableTemplate = template
                                   .Replace(CORE_DB_TABLE_TEMPLATE_PLACEHOLDER.NAMESPACE.Description(), configuration.ORM_Namespace)
                                   .Replace(CORE_DB_TABLE_TEMPLATE_PLACEHOLDER.TABLE_NAME.Description(), table.Name)
                                   .Replace(CORE_DB_TABLE_TEMPLATE_PLACEHOLDER.MODEL_ATTRIBUTES.Description(), modelBuilder.ToString())
                                   .Replace(CORE_DB_TABLE_TEMPLATE_PLACEHOLDER.QUERY_ATTRIBUTES.Description(), queryBuilder.ToString());

                                File.WriteAllText($"{configuration.ORM_Location}\\{table.Name}.cs", tableTemplate);
                            }
                        }

                        #endregion generate orm using template

                        Console.WriteLine($"{nameof(CORE_MySQL_DB_Generator)}: Successfully generated classes!");

                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{nameof(CORE_MySQL_DB_Generator)}: Failed to generate classes!");
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        transaction?.Rollback();

                        connection?.Close();
                    }
                }
            }

            return isSuccess;
        }

        internal static string GetCSharpType(string type)
        {
            // TODO

            return string.Empty;
        }
    }
}