using Core.Shared.Attributes;
using Core.Shared.Database;
using Core.Shared.Utils.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using System.Text;

namespace Core.Shared
{
    /// <summary>
    /// Abstract class that contains all the logic for database table ORM manipulation
    /// </summary>
    /// <typeparam name="T1">Represents main database table model</typeparam>
    /// <typeparam name="T2">Represents model for data querying</typeparam>
    public class ADBTable<T1, T2> where T1 : new() where T2 : new()
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

            using var command = new SqlCommand(queryString, dbConnection.Connection, dbConnection.Transaction);

            var reader = command.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    var item = new T1();

                    foreach (var property in typeof(T1).GetProperties())
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
                var property = typeof(T1).GetProperties().FirstOrDefault(x => x.CustomAttributes.HasValue() && x.CustomAttributes.Any(a => a.AttributeType == typeof(CORE_DB_PrimaryKey)));

                if (property != null)
                {
                    var ids = parameter.Select(x => property.GetValue(x, null)).ToList();

                    if (ids.HasValue() && GetParameterValues(ids, out var parameterValues))
                    {
                        var queryString = $"UPDATE [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] SET [IsDeleted] = 1 WHERE {property.Name} IN ({parameterValues})";

                        using var command = new SqlCommand(queryString, dbConnection.Connection, dbConnection.Transaction);

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

            using var command = new SqlCommand(queryString, dbConnection.Connection, dbConnection.Transaction);

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
                var property = typeof(T1).GetProperties().FirstOrDefault(x => x.CustomAttributes.HasValue() && x.CustomAttributes.Any(a => a.AttributeType == typeof(CORE_DB_PrimaryKey)));

                if (property != null)
                {
                    var ids = parameter.Select(x => property.GetValue(x, null)).ToList();

                    if (ids.HasValue() && GetParameterValues(ids, out var parameterValues))
                    {
                        var queryString = $"DELETE FROM [dbo].[{typeof(T1).DeclaringType?.Name ?? typeof(T1).Name}] WHERE {property.Name} IN ({parameterValues})";

                        using var command = new SqlCommand(queryString, dbConnection.Connection, dbConnection.Transaction);

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

            using var command = new SqlCommand(queryString, dbConnection.Connection, dbConnection.Transaction);

            var result = command.ExecuteNonQuery();

            return result;
        }

        #endregion hard delete

        public static T1 Save(CORE_DB_Connection dbConnection, T1 parameter)
        {
            throw new NotImplementedException();
        }

        internal static string GetColumnsQuery()
        {
            var builder = new StringBuilder();

            foreach (var property in typeof(T1).GetProperties())
            {
                builder.Append($"[{property.Name}], ");
            }

            var columns = builder.ToString();
            columns = columns[..^2];

            return columns;
        }

        internal static string GetWhereCondition(T2 parameter)
        {
            if (parameter == null) { return string.Empty; }

            var builder = new StringBuilder();

            foreach (var property in parameter.GetType().GetProperties())
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

                    segment = $"{segment}, ";

                    builder.Append(segment);
                }
            }

            var where = string.Empty;

            if (builder.Length > 0)
            {
                where = builder.ToString();
                where = where[..^2];
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