using System.Data;
using System.Data.Common;
using System.Text;

namespace Core.DB
{
    public class Get_Products_for_ProductIDs
    {
        public List<MEI_PR_GPfPIDs_1440> Invoke(DbConnection connection, DbTransaction transaction, P_MEI_PR_GPfPIDs_1440 parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            const string commandLocation = "zugseilmeasuringinbll.Operation.DirectDB.Products.SQL.Get_Products_for_ProductIDs.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;

            if (parameter.ProductIDs != null && parameter.ProductIDs.Any())
            {
                command.CommandText = System.Text.RegularExpressions.Regex.Replace(command.CommandText, "=[ \t]*@ProductIDs", " IN $$ProductIDs$$");
                DBSQLSupport.AppendINStatement(command, "$ProductIDs$", parameter.ProductIDs);
            }
            else
            {
                command.CommandText = System.Text.RegularExpressions.Regex.Replace(command.CommandText, "=[ \t]*@ProductIDs", " IS NULL AND FALSE");
                command.CommandText = System.Text.RegularExpressions.Regex.Replace(command.CommandText, "@ProductIDs", "NULL");
            }

            var results = new List<MEI_PR_GPfPIDs_1440_raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new MEI_PR_GPfPIDs_1440_raw();

                    resultItem.CMN_PRO_ProductID = reader.IsDBNull("CMN_PRO_ProductID") ? default : reader.GetGuid("CMN_PRO_ProductID");
                    resultItem.Product_UID = reader.IsDBNull("Product_UID") ? default : reader.GetGuid("Product_UID");
                    resultItem.Product_Number = reader.IsDBNull("Product_Number") ? default : reader.GetString("Product_Number");
                    resultItem.Document_RefID = reader.IsDBNull("Document_RefID") ? default : reader.GetGuid("Document_RefID");
                    resultItem.CutRevision_RefID = reader.IsDBNull("CutRevision_RefID") ? default : reader.GetGuid("CutRevision_RefID");
                    resultItem.CMN_PRO_Product_VariantID = reader.IsDBNull("CMN_PRO_Product_VariantID") ? default : reader.GetGuid("CMN_PRO_Product_VariantID");
                    resultItem.IsStandardProductVariant = reader.IsDBNull("IsStandardProductVariant") ? default : reader.GetBoolean("IsStandardProductVariant");
                    resultItem.IsBlockedForOrdering = reader.IsDBNull("IsBlockedForOrdering") ? default : reader.GetBoolean("IsBlockedForOrdering");
                    resultItem.IsVariantKeptInStock = reader.IsDBNull("IsVariantKeptInStock") ? default : reader.GetBoolean("IsVariantKeptInStock");
                    resultItem.IsBlockedWhenNotOnStock = reader.IsDBNull("IsBlockedWhenNotOnStock") ? default : reader.GetBoolean("IsBlockedWhenNotOnStock");
                    resultItem.MinimumOrderQuantity = reader.IsDBNull("MinimumOrderQuantity") ? default : reader.GetInt32("MinimumOrderQuantity");
                    resultItem.OrderQuantityInterval = reader.IsDBNull("OrderQuantityInterval") ? default : reader.GetInt32("OrderQuantityInterval");
                    resultItem.OrderSequenceNumber = reader.IsDBNull("OrderSequenceNumber") ? default : reader.GetInt32("OrderSequenceNumber");
                    resultItem.CMN_PRO_CUS_CustomizationID = reader.IsDBNull("CMN_PRO_CUS_CustomizationID") ? default : reader.GetGuid("CMN_PRO_CUS_CustomizationID");
                    resultItem.C_OrderSequence = reader.IsDBNull("C_OrderSequence") ? default : reader.GetInt32("C_OrderSequence");
                    resultItem.CMN_PRO_CUS_Customization_VariantID = reader.IsDBNull("CMN_PRO_CUS_Customization_VariantID") ? default : reader.GetGuid("CMN_PRO_CUS_Customization_VariantID");
                    resultItem.CVariant_OrderSequence = reader.IsDBNull("CVariant_OrderSequence") ? default : reader.GetInt32("CVariant_OrderSequence");

                    results.Add(resultItem);
                }

                reader.Close();
            }
            catch (Exception)
            {
                reader.Close();

                throw;
            }

            var result = MEI_PR_GPfPIDs_1440_raw.Convert(results).ToList();

            return result;
        }

        public class P_MEI_PR_GPfPIDs_1440
        {
            public bool IsUseIDs { get; set; }
            public bool IsUseUIDs { get; set; }
            public List<Guid> ProductIDs { get; set; } = new List<Guid>();
        }

        public class MEI_PR_GPfPIDs_1440
        {
            public List<MEI_PR_GPfPIDs_1440_Variant> Variants { get; set; } = new List<MEI_PR_GPfPIDs_1440_Variant>();
            public List<MEI_PR_GPfPIDs_1440_Customization> Customizations { get; set; } = new List<MEI_PR_GPfPIDs_1440_Customization>();

            public Guid CMN_PRO_ProductID { get; set; }
            public Guid Product_UID { get; set; }
            public string Product_Number { get; set; }
            public Guid Document_RefID { get; set; }
            public Guid CutRevision_RefID { get; set; }
        }

        public class MEI_PR_GPfPIDs_1440_Variant
        {
            public Guid CMN_PRO_Product_VariantID { get; set; }
            public bool IsStandardProductVariant { get; set; }
            public bool IsBlockedForOrdering { get; set; }
            public bool IsVariantKeptInStock { get; set; }
            public bool IsBlockedWhenNotOnStock { get; set; }
            public int MinimumOrderQuantity { get; set; }
            public int OrderQuantityInterval { get; set; }
            public int OrderSequenceNumber { get; set; }
        }

        public class MEI_PR_GPfPIDs_1440_Customization
        {
            public List<MEI_PR_GPfPIDs_1440_CustomizationVariant> CustomizationVariants { get; set; } = new List<MEI_PR_GPfPIDs_1440_CustomizationVariant>();

            public Guid CMN_PRO_CUS_CustomizationID { get; set; }
            public int C_OrderSequence { get; set; }
        }

        public class MEI_PR_GPfPIDs_1440_CustomizationVariant
        {
            public Guid CMN_PRO_CUS_Customization_VariantID { get; set; }
            public int CVariant_OrderSequence { get; set; }
        }

        public class MEI_PR_GPfPIDs_1440_raw
        {
            public Guid CMN_PRO_ProductID;
            public Guid Product_UID;
            public string Product_Number;
            public Guid Document_RefID;
            public Guid CutRevision_RefID;
            public Guid CMN_PRO_Product_VariantID;
            public bool IsStandardProductVariant;
            public bool IsBlockedForOrdering;
            public bool IsVariantKeptInStock;
            public bool IsBlockedWhenNotOnStock;
            public int MinimumOrderQuantity;
            public int OrderQuantityInterval;
            public int OrderSequenceNumber;
            public Guid CMN_PRO_CUS_CustomizationID;
            public int C_OrderSequence;
            public Guid CMN_PRO_CUS_Customization_VariantID;
            public int CVariant_OrderSequence;

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            public static List<MEI_PR_GPfPIDs_1440> Convert(List<MEI_PR_GPfPIDs_1440_raw> rawResult)
            {
                var groupResult =

                                  from el_MEI_PR_GPfPIDs_1440 in rawResult

                                  group el_MEI_PR_GPfPIDs_1440 by new
                                  {
                                      el_MEI_PR_GPfPIDs_1440.CMN_PRO_ProductID
                                  }

                                  into gfunct_MEI_PR_GPfPIDs_1440

                                  select new MEI_PR_GPfPIDs_1440
                                  {
                                      CMN_PRO_ProductID = gfunct_MEI_PR_GPfPIDs_1440.Key.CMN_PRO_ProductID,
                                      Product_UID = gfunct_MEI_PR_GPfPIDs_1440.First().Product_UID,
                                      Product_Number = gfunct_MEI_PR_GPfPIDs_1440.First().Product_Number,
                                      Document_RefID = gfunct_MEI_PR_GPfPIDs_1440.First().Document_RefID,
                                      CutRevision_RefID = gfunct_MEI_PR_GPfPIDs_1440.First().CutRevision_RefID,

                                      Variants =
                                      (
                                          from el_Variants in gfunct_MEI_PR_GPfPIDs_1440
                                          group el_Variants by new
                                          {
                                              el_Variants.CMN_PRO_Product_VariantID
                                          }
                                          into gfunct_Variants
                                          select new MEI_PR_GPfPIDs_1440_Variant
                                          {
                                              CMN_PRO_Product_VariantID = gfunct_Variants.Key.CMN_PRO_Product_VariantID,
                                              IsStandardProductVariant = gfunct_Variants.First().IsStandardProductVariant,
                                              IsBlockedForOrdering = gfunct_Variants.First().IsBlockedForOrdering,
                                              IsVariantKeptInStock = gfunct_Variants.First().IsVariantKeptInStock,
                                              IsBlockedWhenNotOnStock = gfunct_Variants.First().IsBlockedWhenNotOnStock,
                                              MinimumOrderQuantity = gfunct_Variants.First().MinimumOrderQuantity,
                                              OrderQuantityInterval = gfunct_Variants.First().OrderQuantityInterval,
                                              OrderSequenceNumber = gfunct_Variants.First().OrderSequenceNumber
                                          }
                                      ).ToList(),

                                      Customizations =
                                      (
                                          from el_Customizations in gfunct_MEI_PR_GPfPIDs_1440
                                          group el_Customizations by new
                                          {
                                              el_Customizations.CMN_PRO_CUS_CustomizationID
                                          }
                                          into gfunct_Customizations
                                          select new MEI_PR_GPfPIDs_1440_Customization
                                          {
                                              CMN_PRO_CUS_CustomizationID = gfunct_Customizations.Key.CMN_PRO_CUS_CustomizationID,
                                              C_OrderSequence = gfunct_Customizations.First().C_OrderSequence,
                                              CustomizationVariants =
                                              (
                                                  from el_CustomizationVariants in gfunct_Customizations
                                                  group el_CustomizationVariants by new
                                                  {
                                                      el_CustomizationVariants.CMN_PRO_CUS_Customization_VariantID
                                                  }
                                                  into gfunct_CustomizationVariants
                                                  select new MEI_PR_GPfPIDs_1440_CustomizationVariant
                                                  {
                                                      CMN_PRO_CUS_Customization_VariantID = gfunct_CustomizationVariants.Key.CMN_PRO_CUS_Customization_VariantID,
                                                      CVariant_OrderSequence = gfunct_CustomizationVariants.First().CVariant_OrderSequence
                                                  }
                                              ).ToList()
                                          }
                                      ).ToList()
                                  };

                return groupResult.ToList();
            }
        }
    }








    public class DBSQLSupport
    {
        public static void AppendINStatement(DbCommand command, string InStatementRegion, Guid[] Parameters, string paramName = null)
        {
            if (paramName == null)
            {
                paramName = InStatementRegion.Trim(new char[1] { '$' });
            }

            string newValue = SQLSupport.GenerateInStatement(Parameters, paramName);
            command.CommandText = command.CommandText.Replace(InStatementRegion, newValue);
            for (int i = 0; i < Parameters.Length; i++)
            {
                SetParameter(command, paramName + i, Parameters[i]);
            }
        }

        public static void AppendINStatement(DbCommand command, string InStatementRegion, List<string> Parameters, string paramName = null)
        {
            if (paramName == null)
            {
                paramName = InStatementRegion.Trim(new char[1] { '$' });
            }

            string newValue = SQLSupport.GenerateInStatement(Parameters, paramName);
            command.CommandText = command.CommandText.Replace(InStatementRegion, newValue);
            for (int i = 0; i < Parameters.Count; i++)
            {
                SetParameter(command, paramName + i, Parameters[i]);
            }
        }

        public static void AppendINStatement(DbCommand command, string InStatementRegion, List<Guid> Parameters, string paramName = null)
        {
            if (paramName == null)
            {
                paramName = InStatementRegion.Trim(new char[1] { '$' });
            }

            string newValue = SQLSupport.GenerateInStatement(Parameters, paramName);
            command.CommandText = command.CommandText.Replace(InStatementRegion, newValue);
            for (int i = 0; i < Parameters.Count; i++)
            {
                SetParameter(command, paramName + i, Parameters[i]);
            }
        }

        public static void AppendINStatement(DbCommand command, string InStatementRegion, string[] Parameters, string paramName = null)
        {
            if (paramName == null)
            {
                paramName = InStatementRegion.Trim(new char[1] { '$' });
            }

            string newValue = SQLSupport.GenerateInStatement(Parameters, paramName);
            command.CommandText = command.CommandText.Replace(InStatementRegion, newValue);
            for (int i = 0; i < Parameters.Length; i++)
            {
                SetParameter(command, paramName + i, Parameters[i]);
            }
        }

        public static void SetParameter(DbCommand command, string name, object value)
        {
            if (value == null) { return; }

            if (value is Guid)
            {
                Guid_SetParameter(command, name, (Guid)value);
            }
            else
            {
                Standard_SetParameter(command, name, value);
            }
        }

        protected static void Guid_SetParameter(DbCommand command, string name, Guid value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value.ToByteArray();
            command.Parameters.Add(parameter);
        }

        protected static void Standard_SetParameter(DbCommand command, string name, object value)
        {
            if (value is DateTime dateTime)
            {
                dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);

                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = dateTime;
                command.Parameters.Add(parameter);
            }
            else
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }

        //public static DbConnection CreateConnection(string Connection)
        //{
        //    return new MySqlConnection(Connection);
        //}

        //public static void SetParameter(DbCommand command, string name, object value)
        //{
        //    MySqlCommand command2 = (MySqlCommand)command;
        //    value?.GetType();
        //    if (value is Guid)
        //    {
        //        Guid_SetParameter(command2, name, (Guid)value);
        //    }
        //    else
        //    {
        //        Standard_SetParameter(command2, name, value);
        //    }
        //}

        //protected static void Guid_SetParameter(MySqlCommand command, string name, Guid value)
        //{
        //    command.Parameters.AddWithValue(name, value.ToByteArray());
        //}

        //protected static void Standard_SetParameter(MySqlCommand command, string name, object value)
        //{
        //    if (value is string)
        //    {
        //        command.Parameters.AddWithValue(name, value);
        //    }
        //    else if (value is DateTime)
        //    {
        //        DateTime dateTime = (DateTime)value;
        //        dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
        //        command.Parameters.AddWithValue(name, dateTime);
        //    }
        //    else
        //    {
        //        command.Parameters.AddWithValue(name, value);
        //    }
        //}
    }




    public static class SQLSupport
    {
        public static string GenerateInStatement(Guid[] guids, string prefix = "D")
        {
            if (guids.Length == 0)
            {
                return "( )";
            }

            StringBuilder stringBuilder = new StringBuilder("( ");
            int num = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                _ = ref guids[i];
                stringBuilder.Append("@" + prefix + num++ + ",");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(" )");
            return stringBuilder.ToString();
        }

        public static string GenerateInStatement(object[] values, string prefix = "D")
        {
            if (values.Length == 0)
            {
                return "( )";
            }

            StringBuilder stringBuilder = new StringBuilder("( ");
            int num = 0;
            for (int i = 0; i < values.Length; i++)
            {
                _ = (string)values[i];
                stringBuilder.Append("@" + prefix + num++ + ",");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(" )");
            return stringBuilder.ToString();
        }

        public static string GenerateInStatement(List<Guid> guids, string prefix = "D")
        {
            if (guids.Count == 0)
            {
                return "( )";
            }

            StringBuilder stringBuilder = new StringBuilder("( ");
            int num = 0;
            foreach (Guid guid in guids)
            {
                _ = guid;
                stringBuilder.Append("@" + prefix + num++ + ",");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(" )");
            return stringBuilder.ToString();
        }

        public static string GenerateInStatement(List<string> values, string prefix = "D")
        {
            if (values.Count == 0)
            {
                return "( )";
            }

            StringBuilder stringBuilder = new StringBuilder("( ");
            int num = 0;
            foreach (string value in values)
            {
                _ = value;
                stringBuilder.Append("@" + prefix + num++ + ",");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(" )");
            return stringBuilder.ToString();
        }

        public static string GenerateInStatement(List<object> values, string prefix = "D")
        {
            if (values.Count == 0)
            {
                return "( )";
            }

            StringBuilder stringBuilder = new StringBuilder("( ");
            int num = 0;
            foreach (string value in values)
            {
                _ = value;
                stringBuilder.Append("@" + prefix + num++ + ",");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(" )");
            return stringBuilder.ToString();
        }
    }
}