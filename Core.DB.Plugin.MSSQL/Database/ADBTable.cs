﻿using Core.DB.Plugin.Shared.Attributes;
using Core.DB.Plugin.Shared.Extensions;
using Core.DB.Plugin.Shared.Interfaces;
using Core.DB.Plugin.Shared.Utils;
using CoreCore.DB.Plugin.Shared.Database;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Core.DB.Plugin.MSSQL.Database
{
    /// <summary>
    /// Abstract class that contains all the logic for database table ORM manipulation
    /// </summary>
    /// <typeparam name="T1">Represents main database table model</typeparam>
    /// <typeparam name="T2">Represents model for data querying</typeparam>
    public class ADBTable<T1, T2> where T1 : IDB_Table, new() where T2 : new()
    {
        /// <summary>
        /// Search all table entries that match values passed by parameter
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<T1> Search(CORE_DB_Connection dbConnection, T2 parameter)
        {
            var result = new List<T1>();

            var queryString = $"SELECT * FROM [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] {GetWhereCondition(parameter)}";

            using var command = new SqlCommand(queryString, (SqlConnection)dbConnection.Connection, (SqlTransaction)dbConnection.Transaction);

            using var reader = command.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    var item = new T1();

                    var properties = typeof(T1).GetFilteredProperties();

                    foreach (var property in properties)
                    {
                        property.SetValue(item, reader[property.Name]);
                    }

                    result.Add(item);
                }
            }
            finally
            {
                reader.Close();
            }

            return result;
        }

        #region soft delete

        /// <summary>
        /// Soft delete single entry
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static int SoftDelete(CORE_DB_Connection dbConnection, T1 parameter)
        {
            return SoftDelete(dbConnection, new List<T1> { parameter });
        }

        /// <summary>
        /// Soft delete multiple entries
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static int SoftDelete(CORE_DB_Connection dbConnection, List<T1> parameter)
        {
            int result = 0;

            if (parameter.HasValue())
            {
                var property = typeof(T1).GetProperties().FirstOrDefault(x => x.CustomAttributes.HasValue() && x.CustomAttributes.Any(a => a.AttributeType == typeof(CORE_DB_SQL_PrimaryKey)));

                if (property != null)
                {
                    var ids = parameter.Select(x => property.GetValue(x, null)).ToList();

                    if (ids.HasValue() && GetParameterValues(ids, out var parameterValues))
                    {
                        var queryString = $"UPDATE [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] SET [IsDeleted] = 1 WHERE {property.Name} IN ({parameterValues})";

                        using var command = new SqlCommand(queryString, (SqlConnection)dbConnection.Connection, (SqlTransaction)dbConnection.Transaction);

                        result = command.ExecuteNonQuery();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Soft delete all entries that match values passed by parameter
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int SoftDelete(CORE_DB_Connection dbConnection, T2 parameter)
        {
            var queryString = $"UPDATE [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] SET [IsDeleted] = 1 {GetWhereCondition(parameter)}";

            using var command = new SqlCommand(queryString, (SqlConnection)dbConnection.Connection, (SqlTransaction)dbConnection.Transaction);

            var result = command.ExecuteNonQuery();

            return result;
        }

        #endregion soft delete

        #region hard delete

        /// <summary>
        /// Hard delete single entry
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static int Delete(CORE_DB_Connection dbConnection, T1 parameter)
        {
            return Delete(dbConnection, new List<T1> { parameter });
        }

        /// <summary>
        /// Hard delete multiple entries
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int Delete(CORE_DB_Connection dbConnection, List<T1> parameter)
        {
            int result = 0;

            if (parameter.HasValue())
            {
                var property = typeof(T1).GetProperties().FirstOrDefault(x => x.CustomAttributes.HasValue() && x.CustomAttributes.Any(a => a.AttributeType == typeof(CORE_DB_SQL_PrimaryKey)));

                if (property != null)
                {
                    var ids = parameter.Select(x => property.GetValue(x, null)).ToList();

                    if (ids.HasValue() && GetParameterValues(ids, out var parameterValues))
                    {
                        var queryString = $"DELETE FROM [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] WHERE {property.Name} IN ({parameterValues})";

                        using var command = new SqlCommand(queryString, (SqlConnection)dbConnection.Connection, (SqlTransaction)dbConnection.Transaction);

                        result = command.ExecuteNonQuery();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hard delete all entries that match values passed by parameter
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static int Delete(CORE_DB_Connection dbConnection, T2 parameter)
        {
            var queryString = $"DELETE FROM [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] {GetWhereCondition(parameter)}";

            using var command = new SqlCommand(queryString, (SqlConnection)dbConnection.Connection, (SqlTransaction)dbConnection.Transaction);

            var result = command.ExecuteNonQuery();

            return result;
        }

        #endregion hard delete

        /// <summary>
        /// isUsingPrimaryKeyAutoIncrement specifies that MSSQL database is configured to automatically generate primary key values so they should be excluded from insert statement
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="parameter"></param>
        /// <param name="isUsingPrimaryKeyAutoIncrement"></param>
        /// <returns></returns>
        public static T1 Save(CORE_DB_Connection dbConnection, T1 parameter, bool isUsingPrimaryKeyAutoIncrement = false)
        {
            var primaryKeyProperty = parameter.GetPrimaryKeyProperty() ?? throw new Exception($"Primary key property not found for type {parameter.GetType().Name}");

            var usingStatement = GetUsingPartForMergeStatement(parameter);
            var (matched, notMatchedColumns, notMatchedValues) = OnDuplicateKeyStatement(primaryKeyProperty, isUsingPrimaryKeyAutoIncrement);

            var queryString = $"MERGE INTO [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] AS target"
                +
                $" USING (SELECT {usingStatement}) as source"
                +
                $" ON target.[{primaryKeyProperty.Name}] = source.[{primaryKeyProperty.Name}]"
                +
                $" WHEN MATCHED THEN UPDATE SET {matched}"
                +
                $" WHEN NOT MATCHED THEN INSERT ({notMatchedColumns}) VALUES ({notMatchedValues}); SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(queryString, (SqlConnection)dbConnection.Connection, (SqlTransaction)dbConnection.Transaction);

            var result = command.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                var id = Convert.ChangeType(result, primaryKeyProperty.PropertyType);

                primaryKeyProperty.SetValue(parameter, id);
            }

            return parameter;
        }

        internal static (string columns, string values) GetColumnsAndValues(T1 parameter)
        {
            var properties = typeof(T1).GetFilteredProperties();

            var columnsBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();

            foreach (var property in properties)
            {
                columnsBuilder.Append($"[{property.Name}], ");

                var value = property.GetValue(parameter, null);

                if (value == null)
                {
                    valuesBuilder.Append("NULL, ");
                    continue;
                }

                var valueType = value.GetType();

                string segment;

                if
                (
                        valueType == typeof(double) ||
                        valueType == typeof(float) ||
                        valueType == typeof(decimal) ||
                        valueType == typeof(int)
                )
                {
                    segment = $"{value}";
                }
                else if (valueType == typeof(DateTime))
                {
                    segment = $"'{(DateTime)value:yyyy-MM-dd HH:mm:ss}'";
                }
                else if (valueType == typeof(bool))
                {
                    segment = (bool)value ? "1" : "0";
                }
                else
                {
                    segment = $"'{value}'";
                }

                segment = $"{segment}, ";

                valuesBuilder.Append(segment);
            }

            var columns = columnsBuilder.ToString();
            columns = columns[..^2];

            var values = valuesBuilder.ToString();
            values = values[..^2];

            return (columns, values);
        }

        internal static string GetUsingPartForMergeStatement(T1 parameter)
        {
            var properties = typeof(T1).GetFilteredProperties();

            var sb = new StringBuilder();

            foreach (var property in properties)
            {
                var value = property.GetValue(parameter, null);

                string segment;
                if (value == null)
                {
                    sb.Append($"NULL as [{property.Name}], ");

                    continue;
                }

                var valueType = value.GetType();

                if
                (
                    valueType == typeof(double) ||
                    valueType == typeof(float) ||
                    valueType == typeof(decimal) ||
                    valueType == typeof(int)
                )
                {
                    segment = $"{value}";
                }
                else if (valueType == typeof(DateTime))
                {
                    segment = $"'{(DateTime)value:yyyy-MM-dd HH:mm:ss}'";
                }
                else if (valueType == typeof(bool))
                {
                    segment = (bool)value ? "1" : "0";
                }
                else
                {
                    segment = $"'{value}'";
                }

                sb.Append($"{segment} as [{property.Name}], ");
            }

            var result = sb.ToString();
            result = result[..^2];

            return result;
        }

        internal static (string matched, string notMatchedColumns, string notMatchedValues) OnDuplicateKeyStatement(PropertyInfo primaryKeyProperty, bool isUsingPrimaryKeyAutoIncrement)
        {
            var properties = typeof(T1).GetFilteredProperties();

            var sbMatched = new StringBuilder();
            var sbNotMatchedColumns = new StringBuilder();
            var sbNotMatchedValues = new StringBuilder();

            foreach (var property in properties)
            {
                var isPrimaryKey = property == primaryKeyProperty;

                if (!isPrimaryKey)
                {
                    sbMatched.Append($"target.{property.Name} = source.{property.Name}, ");
                }

                if (!(isPrimaryKey && isUsingPrimaryKeyAutoIncrement) || !isPrimaryKey)
                {
                    sbNotMatchedColumns.Append($"{property.Name}, ");
                    sbNotMatchedValues.Append($"source.{property.Name}, ");
                }
            }

            var matched = sbMatched.ToString();
            matched = matched[..^2];

            var notMatchedColumns = sbNotMatchedColumns.ToString();
            notMatchedColumns = notMatchedColumns[..^2];

            var notMatchedValues = sbNotMatchedValues.ToString();
            notMatchedValues = notMatchedValues[..^2];

            return (matched, notMatchedColumns, notMatchedValues);
        }

        internal static string GetWhereCondition(T2 parameter)
        {
            if (parameter == null) { return string.Empty; }

            var builder = new StringBuilder();

            var properties = parameter.GetType().GetFilteredProperties();

            foreach (var property in properties)
            {
                if (property.GetValue(parameter, null) != null)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append("WHERE ");
                    }

                    string segment;

                    if (property.PropertyType == typeof(DateTime?))
                    {
                        var dateParameter = ((DateTime?)property.GetValue(parameter, null))?.ToString("yyyy-MM-dd HH:mm:ss") ?? "0";

                        segment = $"{property.Name} = '{dateParameter}'";
                    }
                    else if (property.PropertyType == typeof(bool?))
                    {
                        if ((bool?)property.GetValue(parameter, null) == true)
                        {
                            segment = $"{property.Name} = 1";
                        }
                        else
                        {
                            segment = $"{property.Name} = 0";
                        }
                    }
                    else if
                        (
                        property.PropertyType == typeof(double?) ||
                        property.PropertyType == typeof(float?) ||
                        property.PropertyType == typeof(decimal?) ||
                        property.PropertyType == typeof(int?)
                        )
                    {
                        segment = $"{property.Name} = {property.GetValue(parameter, null)}";
                    }
                    else
                    {
                        segment = $"{property.Name} = '{property.GetValue(parameter, null)}'";
                    }

                    segment = $"{segment} AND ";

                    builder.Append(segment);
                }
            }

            var where = string.Empty;

            if (builder.Length > 0)
            {
                where = builder.ToString();
                where = where[..^5];
            }

            return where;
        }

        internal static bool GetParameterValues(List<object?> values, out string? parameterValues)
        {
            var builder = new StringBuilder();

            if (values.HasValue())
            {
                foreach (var value in values)
                {
                    if (value == null) { continue; }

                    var valueType = value.GetType();

                    string segment;

                    if
                        (
                        valueType == typeof(double) ||
                        valueType == typeof(float) ||
                        valueType == typeof(decimal) ||
                        valueType == typeof(int)
                        )
                    {
                        segment = $"{value}";
                    }
                    else
                    {
                        segment = $"'{value}'";
                    }

                    segment = $"{segment}, ";

                    builder.Append(segment);
                }
            }

            if (builder.Length > 0)
            {
                parameterValues = builder.ToString();
                parameterValues = parameterValues[..^2];

                return true;
            }
            else
            {
                parameterValues = null;
            }

            return false;
        }
    }
}