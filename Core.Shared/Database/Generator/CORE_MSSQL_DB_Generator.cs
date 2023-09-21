using Core.Shared.Configuration;
using Core.Shared.Database.Generator.Enumerations;
using Core.Shared.Utils.Extensions;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Core.Shared.Database.Generator
{
    public static class CORE_MSSQL_DB_Generator
    {
        public static void GenerateORMs_FromMSSQL(CORE_DB_Connection dbConnection, ICORE_DB_GENERATOR_Configuration configuration)
        {
            #region load table names

            var query_GetTables = "SELECT * FROM SYSOBJECTS WHERE xtype = 'U';";

            using var command_GetTables = new SqlCommand(query_GetTables, dbConnection.Connection, dbConnection.Transaction);

            using var reader_GetTable = command_GetTables.ExecuteReader();

            var tableNames = new List<string>();

            while (reader_GetTable.Read())
            {
                var value = reader_GetTable.GetString(reader_GetTable.GetOrdinal("name"));

                if (!string.IsNullOrEmpty(value))
                {
                    tableNames.Add(value);
                }
            }

            reader_GetTable.Close();

            if (!tableNames.HasValue()) { Console.WriteLine("No tables were found to generate ORMs"); return; }

            #endregion load table names

            #region load tables data

            var tables = new List<CORE_DB_TABLE>();

            foreach (var tableName in tableNames)
            {
                var query_GetTableData = $"exec sp_columns {tableName}";

                using var command_GetTableData = new SqlCommand(query_GetTableData, dbConnection.Connection, dbConnection.Transaction);

                using var reader_GetTableData = command_GetTableData.ExecuteReader();

                var table = new CORE_DB_TABLE
                {
                    Name = tableName
                };

                while (reader_GetTableData.Read())
                {
                    table.Columns.Add(new CORE_DB_TABLE_COLUMN
                    {
                        Name = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("COLUMN_NAME")),
                        TypeName = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("TYPE_NAME")),
                        DataType = reader_GetTableData.GetInt16(reader_GetTableData.GetOrdinal("DATA_TYPE")),
                        IsNullable = reader_GetTableData.GetString(reader_GetTableData.GetOrdinal("IS_NULLABLE")) != "NO",
                        OrdinalPosition = reader_GetTableData.GetInt32(reader_GetTableData.GetOrdinal("ORDINAL_POSITION"))
                    });
                }

                reader_GetTableData.Close();

                if (!table.Columns.HasValue()) { continue; }

                tables.Add(table);
            }

            if (!tables.HasValue()) { Console.WriteLine("No table data was found to generate ORMs"); return; }

            #endregion load tables data

            #region generate orm using template

            var template = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? string.Empty, "Database\\Generator\\Templates\\DB_TABLE_TEMPLATE.txt"));

            try { Directory.Delete(configuration.ORM_Location, true); } catch (Exception) { }
            try { Directory.CreateDirectory(configuration.ORM_Location); } catch (Exception) { }

            foreach (var table in tables)
            {
                if (table.Columns.HasValue())
                {
                    var modelBuilder = new StringBuilder();
                    var queryBuilder = new StringBuilder();

                    foreach (var column in table.Columns.OrderBy(x => x.OrdinalPosition).ToList())
                    {
                        // primary key
                        if (column.OrdinalPosition == 1 && (column.DataType == -2 || column.DataType == -11))
                        {
                            modelBuilder.AppendLine($"        public Guid {column.Name} {{ get; set; }} = Guid.NewGuid();");
                        }
                        else
                        {
                            if (column.IsNullable)
                            {
                                modelBuilder.AppendLine($"        public {GetCSharpType(column.DataType)}? {column.Name} {{ get; set; }}");
                            }
                            else
                            {
                                if (IsValueType(column.DataType))
                                {
                                    modelBuilder.AppendLine($"        public {GetCSharpType(column.DataType)} {column.Name} {{ get; set; }}");
                                }
                                else
                                {
                                    modelBuilder.AppendLine($"        public {GetCSharpType(column.DataType)} {column.Name} {{ get; set; }} = null!;");
                                }
                            }
                        }

                        queryBuilder.AppendLine($"        public {GetCSharpType(column.DataType)}? {column.Name} {{ get; set; }} = null;");
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
        }

        internal static bool IsValueType(int ODBC_Code)
        {
            return
                ODBC_Code == ODBC_DATA_TYPE.Binary.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.UniqueIdentifier.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Date.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Time.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Timestamp.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Decimal.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Integer.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.SmallInt.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Tinyint.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Float.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Double.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.BigInt.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Bit.IntegerDescription();
        }

        internal static string GetCSharpType(int ODBC_Code)
        {
            if (ODBC_Code == ODBC_DATA_TYPE.Binary.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.UniqueIdentifier.IntegerDescription())
            {
                return "Guid";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Date.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Time.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Timestamp.IntegerDescription())
            {
                return "DateTime";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Decimal.IntegerDescription())
            {
                return "decimal";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Integer.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.SmallInt.IntegerDescription() ||
                ODBC_Code == ODBC_DATA_TYPE.Tinyint.IntegerDescription())
            {
                return "int";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Float.IntegerDescription())
            {
                return "float";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Double.IntegerDescription())
            {
                return "double";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.BigInt.IntegerDescription())
            {
                return "long";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Bit.IntegerDescription())
            {
                return "bool";
            }
            else if (ODBC_Code == ODBC_DATA_TYPE.Char.IntegerDescription()
                || ODBC_Code == ODBC_DATA_TYPE.Varchar.IntegerDescription()
                || ODBC_Code == ODBC_DATA_TYPE.WVarchar.IntegerDescription()
                || ODBC_Code == ODBC_DATA_TYPE.WLongvarchar.IntegerDescription()
                || ODBC_Code == ODBC_DATA_TYPE.WChar.IntegerDescription())
            {
                return "string";
            }
            else
            {
                return "object";
            }
        }
    }

    internal class CORE_DB_TABLE
    {
        public string Name { get; set; } = null!;
        public List<CORE_DB_TABLE_COLUMN> Columns { get; set; } = new List<CORE_DB_TABLE_COLUMN>();
    }

    internal class CORE_DB_TABLE_COLUMN
    {
        public string Name { get; set; } = null!;
        public string TypeName { get; set; } = null!;
        public int DataType { get; set; }
        public int OrdinalPosition { get; set; }
        public bool IsNullable { get; set; }
    }

    internal enum ODBC_DATA_TYPE
    {
        [Description("1")]
        Char,

        [Description("3")]
        Decimal,

        [Description("8")]
        Double,

        [Description("6")]
        Float,

        [Description("4")]
        Integer,

        [Description("2")]
        Numeric,

        [Description("7")]
        Real,

        [Description("5")]
        SmallInt,

        [Description("12")]
        Varchar,

        [Description("-8")]
        WChar,

        [Description("-9")]
        WVarchar,

        [Description("-10")]
        WLongvarchar,



        [Description("-5")]
        BigInt,

        [Description("-2")]
        Binary,

        [Description("-7")]
        Bit,

        [Description("9")]
        Date,

        [Description("10")]
        Time,

        [Description("11")]
        Timestamp,

        [Description("-6")]
        Tinyint,

        [Description("-11")]
        UniqueIdentifier
    }
}