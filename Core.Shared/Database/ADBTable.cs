using Core.Shared.Configuration;
using System.Data.SqlClient;
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
        // TODO - to be replaced with transactionManager -> reuse connection and transaction
#warning must be replaced with transactionManager -> reuse connection and transaction
        public static List<T1> Search(CORE_Database dbConfiguration, T2 parameter)
        {
            var result = new List<T1>();

            string queryString = $"SELECT * FROM [dbo].[{typeof(T1).Name}] {GetWhereCondition(parameter)}";

            using SqlConnection connection = new(dbConfiguration.ConnectionString);
            connection.Open();

            var command = new SqlCommand(queryString, connection);

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
                // Always call Close when done reading.
                reader.Close();
            }

            return result;
        }

        public static int SoftDelete(T1 parameter)
        {
            return SoftDelete(new List<T1> { parameter });
        }

        public static int Delete(T1 parameter)
        {
            return Delete(new List<T1> { parameter });
        }

        public static int SoftDelete(List<T1> parameter)
        {
            throw new NotImplementedException();
        }

        public static int Delete(List<T1> parameter)
        {
            throw new NotImplementedException();
        }

        public static int SoftDelete(T2 parameter)
        {
            throw new NotImplementedException();
        }

        public static int Delete(T2 parameter)
        {
            throw new NotImplementedException();
        }

        public static T1 Save(T1 parameter)
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
                        property.PropertyType == typeof(double) ||
                        property.PropertyType == typeof(float) ||
                        property.PropertyType == typeof(decimal) ||
                        property.PropertyType == typeof(int)
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
    }
}