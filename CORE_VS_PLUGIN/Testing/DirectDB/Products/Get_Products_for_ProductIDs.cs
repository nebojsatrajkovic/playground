using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace zugseilmeasuringinbll.Operation.DirectDB.Products
{
    public class Get_Products_for_ProductIDs
    {
        public List<MEI_PR_GPfPIDs_1440> Invoke(DbConnection connection, DbTransaction transaction, P_MEI_PR_GPfPIDs_1440 parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            const string commandLocation = "CORE_VS_PLUGIN.Testing.DirectDB.Products.Get_Products_for_ProductIDs.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;
            // TODO set parameters
            var results = new List<MEI_PR_GPfPIDs_1440_Raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new MEI_PR_GPfPIDs_1440_Raw();
                    results.Add(resultItem);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                reader.Close();
                throw;
            }

            var result = MEI_PR_GPfPIDs_1440_Raw.Convert(results).ToList();
            return result;
        }

        public class MEI_PR_GPfPIDs_1440_Raw
        {
            public Guid CMN_PRO_ProductID { get; set; }
            public Guid Product_UID { get; set; }
            public string Product_Number { get; set; }
            public Guid Document_RefID { get; set; }
            public Guid CutRevision_RefID { get; set; }
            public Guid CMN_PRO_Product_VariantID { get; set; }
            public bool IsStandardProductVariant { get; set; }
            public bool IsBlockedForOrdering { get; set; }
            public bool IsVariantKeptInStock { get; set; }
            public bool IsBlockedWhenNotOnStock { get; set; }
            public int MinimumOrderQuantity { get; set; }
            public int OrderQuantityInterval { get; set; }
            public int OrderSequenceNumber { get; set; }
            public Guid CMN_PRO_CUS_CustomizationID { get; set; }
            public int C_OrderSequence { get; set; }
            public Guid CMN_PRO_CUS_Customization_VariantID { get; set; }
            public int CVariant_OrderSequence { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            public static List<MEI_PR_GPfPIDs_1440> Convert(List<MEI_PR_GPfPIDs_1440_Raw> rawResult)
            {
                var groupResult =
                    from el_MEI_PR_GPfPIDs_1440 in rawResult.Where(element => !EqualsDefaultValue(element.CMN_PRO_ProductID)).ToList()group el_MEI_PR_GPfPIDs_1440 by new
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
                            Variants = (
                                from el_Variants in gfunct_MEI_PR_GPfPIDs_1440.Where(element => !EqualsDefaultValue(element.CMN_PRO_Product_VariantID)).ToList()group el_Variants by new
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
                                        OrderSequenceNumber = gfunct_Variants.First().OrderSequenceNumber,
                                    }

                            ).ToList(),
                            Customizations = (
                                from el_Customizations in gfunct_MEI_PR_GPfPIDs_1440.Where(element => !EqualsDefaultValue(element.CMN_PRO_CUS_CustomizationID)).ToList()group el_Customizations by new
                                {
                                    el_Customizations.CMN_PRO_CUS_CustomizationID
                                }

                                    into gfunct_Customizations
                                    select new MEI_PR_GPfPIDs_1440_Customization
                                    {
                                        CMN_PRO_CUS_CustomizationID = gfunct_Customizations.Key.CMN_PRO_CUS_CustomizationID,
                                        C_OrderSequence = gfunct_Customizations.First().C_OrderSequence,
                                        CustomizationVariants = (
                                            from el_CustomizationVariants in gfunct_Customizations.Where(element => !EqualsDefaultValue(element.CMN_PRO_CUS_Customization_VariantID)).ToList()group el_CustomizationVariants by new
                                            {
                                                el_CustomizationVariants.CMN_PRO_CUS_Customization_VariantID
                                            }

                                                into gfunct_CustomizationVariants
                                                select new MEI_PR_GPfPIDs_1440_CustomizationVariant
                                                {
                                                    CMN_PRO_CUS_Customization_VariantID = gfunct_CustomizationVariants.Key.CMN_PRO_CUS_Customization_VariantID,
                                                    CVariant_OrderSequence = gfunct_CustomizationVariants.First().CVariant_OrderSequence,
                                                }

                                        ).ToList(),
                                    }

                            ).ToList(),
                        };
                return groupResult.ToList();
            }
        }

        public class P_MEI_PR_GPfPIDs_1440
        {
            public bool IsUseIDs { get; set; }
            public bool IsUseUIDs { get; set; }
            public List<Guid> ProductIDs { get; set; }
        }

        public class MEI_PR_GPfPIDs_1440
        {
            public Guid CMN_PRO_ProductID { get; set; }
            public Guid Product_UID { get; set; }
            public string Product_Number { get; set; }
            public Guid Document_RefID { get; set; }
            public Guid CutRevision_RefID { get; set; }
            public List<MEI_PR_GPfPIDs_1440_Variant> Variants { get; set; }
            public List<MEI_PR_GPfPIDs_1440_Customization> Customizations { get; set; }
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
            public Guid CMN_PRO_CUS_CustomizationID { get; set; }
            public int C_OrderSequence { get; set; }
            public List<MEI_PR_GPfPIDs_1440_CustomizationVariant> CustomizationVariants { get; set; }
        }

        public class MEI_PR_GPfPIDs_1440_CustomizationVariant
        {
            public Guid CMN_PRO_CUS_Customization_VariantID { get; set; }
            public int CVariant_OrderSequence { get; set; }
        }
    }
}