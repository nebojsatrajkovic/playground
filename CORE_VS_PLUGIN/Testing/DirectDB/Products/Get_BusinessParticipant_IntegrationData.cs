using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace zugseilmeasuringinbll.Operation.DirectDB.Products
{
    public class Get_BusinessParticipant_IntegrationData
    {
        public BPT_BP_GBPID_1114 Invoke(DbConnection connection, DbTransaction transaction, P_BPT_BP_GBPID_1114 parameter)
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            const string commandLocation = "CORE_VS_PLUGIN.Testing.DirectDB.Products.Get_BusinessParticipant_IntegrationData.sql";
            command.CommandText = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(commandLocation)).ReadToEnd();
            command.CommandTimeout = 60;
            // TODO set parameters
            var results = new List<BPT_BP_GBPID_1114_Raw>();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var resultItem = new BPT_BP_GBPID_1114_Raw();

                    resultItem.CMN_BPT_BusinessParticipantID = reader.IsDBNull("CMN_BPT_BusinessParticipantID") ? default : reader.GetGuid("CMN_BPT_BusinessParticipantID");

                    //resultItem.BOP_NetworkRoleID = reader.IsDBNull("BOP_NetworkRoleID") ? default : reader.GetGuid("BOP_NetworkRoleID");
                    //resultItem.NetworkRole_GID = reader.IsDBNull("NetworkRole_GID") ? default : reader.GetGuid("NetworkRole_GID");
                    //resultItem.Relation_Role_Name = reader.IsDBNull("Relation_Role_Name") ? default : reader.GetString("Relation_Role_Name");
                    //resultItem.BOP_InterIdentityRelationTypeID = reader.IsDBNull("BOP_InterIdentityRelationTypeID") ? default : reader.GetGuid("BOP_InterIdentityRelationTypeID");
                    //resultItem.Relation_Type_Title = reader.IsDBNull("Relation_Type_Title") ? default : reader.GetString("Relation_Type_Title");
                    //resultItem.BOP_InterIdentityRelationID = reader.IsDBNull("BOP_InterIdentityRelationID") ? default : reader.GetGuid("BOP_InterIdentityRelationID");
                    //resultItem.BOP_InterIdentityRelation_Role_AssignmentID = reader.IsDBNull("BOP_InterIdentityRelation_Role_AssignmentID") ? default : reader.GetGuid("BOP_InterIdentityRelation_Role_AssignmentID");
                    //resultItem.BOP_InterIdentityRelation_Role_Assignment_StatusID = reader.IsDBNull("BOP_InterIdentityRelation_Role_Assignment_StatusID") ? default : reader.GetGuid("BOP_InterIdentityRelation_Role_Assignment_StatusID");
                    //resultItem.Relation_Role_Assignment_Status_GID = reader.IsDBNull("Relation_Role_Assignment_Status_GID") ? default : reader.GetGuid("Relation_Role_Assignment_Status_GID");
                    //resultItem.Relation_Role_Assignment_Status_Name = reader.IsDBNull("Relation_Role_Assignment_Status_Name") ? default : reader.GetString("Relation_Role_Assignment_Status_Name");
                    //resultItem.BOP_Relation_Roles_TrustLevelID = reader.IsDBNull("BOP_Relation_Roles_TrustLevelID") ? default : reader.GetGuid("BOP_Relation_Roles_TrustLevelID");
                    //resultItem.Relation_Roles_TrustLevel_GID = reader.IsDBNull("Relation_Roles_TrustLevel_GID") ? default : reader.GetGuid("Relation_Roles_TrustLevel_GID");
                    //resultItem.Relation_Roles_TrustLevel_Title = reader.IsDBNull("Relation_Roles_TrustLevel_Title") ? default : reader.GetString("Relation_Roles_TrustLevel_Title");
                    //resultItem.LOG_WRH_WarehouseID = reader.IsDBNull("LOG_WRH_WarehouseID") ? default : reader.GetGuid("LOG_WRH_WarehouseID");
                    //resultItem.CoordinateCode = reader.IsDBNull("CoordinateCode") ? default : reader.GetString("CoordinateCode");
                    //resultItem.Warehouse_Name = reader.IsDBNull("Warehouse_Name") ? default : reader.GetString("Warehouse_Name");
                    //resultItem.TransferCustomerOwnedStockInformation = reader.IsDBNull("TransferCustomerOwnedStockInformation") ? default : reader.GetBoolean("TransferCustomerOwnedStockInformation");
                    //resultItem.TransferSelfOwnedStockInformation = reader.IsDBNull("TransferSelfOwnedStockInformation") ? default : reader.GetBoolean("TransferSelfOwnedStockInformation");
                    //resultItem.InternalNetworkRoleID = reader.IsDBNull("InternalNetworkRoleID") ? default : reader.GetGuid("InternalNetworkRoleID");
                    //resultItem.Role_Name = reader.IsDBNull("Role_Name") ? default : reader.GetString("Role_Name");

                    results.Add(resultItem);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                reader.Close();
                throw;
            }

            var result = BPT_BP_GBPID_1114_Raw.Convert(results).FirstOrDefault();
            return result;
        }

        public class BPT_BP_GBPID_1114_Raw
        {
            public Guid CMN_BPT_BusinessParticipantID { get; set; }
            public Guid BOP_NetworkRoleID { get; set; }
            public Guid NetworkRole_GID { get; set; }
            public string Relation_Role_Name { get; set; }
            public Guid BOP_InterIdentityRelationTypeID { get; set; }
            public string Relation_Type_Title { get; set; }
            public Guid BOP_InterIdentityRelationID { get; set; }
            public Guid BOP_InterIdentityRelation_Role_AssignmentID { get; set; }
            public Guid BOP_InterIdentityRelation_Role_Assignment_StatusID { get; set; }
            public Guid Relation_Role_Assignment_Status_GID { get; set; }
            public string Relation_Role_Assignment_Status_Name { get; set; }
            public Guid BOP_Relation_Roles_TrustLevelID { get; set; }
            public Guid Relation_Roles_TrustLevel_GID { get; set; }
            public string Relation_Roles_TrustLevel_Title { get; set; }
            public Guid LOG_WRH_WarehouseID { get; set; }
            public string CoordinateCode { get; set; }
            public string Warehouse_Name { get; set; }
            public bool TransferCustomerOwnedStockInformation { get; set; }
            public bool TransferSelfOwnedStockInformation { get; set; }
            public Guid InternalNetworkRoleID { get; set; }
            public string Role_Name { get; set; }

            private static bool EqualsDefaultValue<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            public static List<BPT_BP_GBPID_1114> Convert(List<BPT_BP_GBPID_1114_Raw> rawResult)
            {
                var groupResult =
                    from el_BPT_BP_GBPID_1114 in rawResult.Where(element => !EqualsDefaultValue(element.CMN_BPT_BusinessParticipantID)).ToList()group el_BPT_BP_GBPID_1114 by new
                    {
                        el_BPT_BP_GBPID_1114.CMN_BPT_BusinessParticipantID
                    }

                        into gfunct_BPT_BP_GBPID_1114
                        select new BPT_BP_GBPID_1114
                        {
                            CMN_BPT_BusinessParticipantID = gfunct_BPT_BP_GBPID_1114.Key.CMN_BPT_BusinessParticipantID,
                            IntertenantRoles = (
                                from el_IntertenantRoles in gfunct_BPT_BP_GBPID_1114.Where(element => !EqualsDefaultValue(element.BOP_InterIdentityRelation_Role_AssignmentID)).ToList()group el_IntertenantRoles by new
                                {
                                    el_IntertenantRoles.BOP_InterIdentityRelation_Role_AssignmentID
                                }

                                    into gfunct_IntertenantRoles
                                    select new BPT_BP_GBPID_1114_IntertenantRole
                                    {
                                        BOP_InterIdentityRelation_Role_AssignmentID = gfunct_IntertenantRoles.Key.BOP_InterIdentityRelation_Role_AssignmentID,
                                        BOP_NetworkRoleID = gfunct_IntertenantRoles.First().BOP_NetworkRoleID,
                                        NetworkRole_GID = gfunct_IntertenantRoles.First().NetworkRole_GID,
                                        Relation_Role_Name = gfunct_IntertenantRoles.First().Relation_Role_Name,
                                        BOP_InterIdentityRelationTypeID = gfunct_IntertenantRoles.First().BOP_InterIdentityRelationTypeID,
                                        Relation_Type_Title = gfunct_IntertenantRoles.First().Relation_Type_Title,
                                        BOP_InterIdentityRelationID = gfunct_IntertenantRoles.First().BOP_InterIdentityRelationID,
                                        BOP_InterIdentityRelation_Role_Assignment_StatusID = gfunct_IntertenantRoles.First().BOP_InterIdentityRelation_Role_Assignment_StatusID,
                                        Relation_Role_Assignment_Status_GID = gfunct_IntertenantRoles.First().Relation_Role_Assignment_Status_GID,
                                        Relation_Role_Assignment_Status_Name = gfunct_IntertenantRoles.First().Relation_Role_Assignment_Status_Name,
                                        TrustLevels = (
                                            from el_TrustLevels in gfunct_IntertenantRoles.Where(element => !EqualsDefaultValue(element.BOP_Relation_Roles_TrustLevelID)).ToList()group el_TrustLevels by new
                                            {
                                                el_TrustLevels.BOP_Relation_Roles_TrustLevelID
                                            }

                                                into gfunct_TrustLevels
                                                select new BPT_BP_GBPID_1114_TrustLevel
                                                {
                                                    BOP_Relation_Roles_TrustLevelID = gfunct_TrustLevels.Key.BOP_Relation_Roles_TrustLevelID,
                                                    Relation_Roles_TrustLevel_GID = gfunct_TrustLevels.First().Relation_Roles_TrustLevel_GID,
                                                    Relation_Roles_TrustLevel_Title = gfunct_TrustLevels.First().Relation_Roles_TrustLevel_Title,
                                                    TrustLevel_SharedWarehouses = (
                                                        from el_TrustLevel_SharedWarehouses in gfunct_TrustLevels.Where(element => !EqualsDefaultValue(element.LOG_WRH_WarehouseID)).ToList()group el_TrustLevel_SharedWarehouses by new
                                                        {
                                                            el_TrustLevel_SharedWarehouses.LOG_WRH_WarehouseID
                                                        }

                                                            into gfunct_TrustLevel_SharedWarehouses
                                                            select new BPT_BP_GBPID_1114_TrustLevel_SharedWarehouse
                                                            {
                                                                LOG_WRH_WarehouseID = gfunct_TrustLevel_SharedWarehouses.Key.LOG_WRH_WarehouseID,
                                                                CoordinateCode = gfunct_TrustLevel_SharedWarehouses.First().CoordinateCode,
                                                                Warehouse_Name = gfunct_TrustLevel_SharedWarehouses.First().Warehouse_Name,
                                                                TransferCustomerOwnedStockInformation = gfunct_TrustLevel_SharedWarehouses.First().TransferCustomerOwnedStockInformation,
                                                                TransferSelfOwnedStockInformation = gfunct_TrustLevel_SharedWarehouses.First().TransferSelfOwnedStockInformation,
                                                            }

                                                    ).ToList(),
                                                }

                                        ).ToList(),
                                    }

                            ).ToList(),
                            InternalRoles = (
                                from el_InternalRoles in gfunct_BPT_BP_GBPID_1114.Where(element => !EqualsDefaultValue(element.InternalNetworkRoleID)).ToList()group el_InternalRoles by new
                                {
                                    el_InternalRoles.InternalNetworkRoleID
                                }

                                    into gfunct_InternalRoles
                                    select new BPT_BP_GBPID_1114_InternalRole
                                    {
                                        InternalNetworkRoleID = gfunct_InternalRoles.Key.InternalNetworkRoleID,
                                        Role_Name = gfunct_InternalRoles.First().Role_Name,
                                    }

                            ).ToList(),
                        };
                return groupResult.ToList();
            }
        }

        public class P_BPT_BP_GBPID_1114
        {
            public Guid ID { get; set; }
        }

        public class BPT_BP_GBPID_1114
        {
            public Guid CMN_BPT_BusinessParticipantID { get; set; }
            public List<BPT_BP_GBPID_1114_IntertenantRole> IntertenantRoles { get; set; }
            public List<BPT_BP_GBPID_1114_InternalRole> InternalRoles { get; set; }
        }

        public class BPT_BP_GBPID_1114_IntertenantRole
        {
            public Guid BOP_NetworkRoleID { get; set; }
            public Guid NetworkRole_GID { get; set; }
            public string Relation_Role_Name { get; set; }
            public Guid BOP_InterIdentityRelationTypeID { get; set; }
            public string Relation_Type_Title { get; set; }
            public Guid BOP_InterIdentityRelationID { get; set; }
            public Guid BOP_InterIdentityRelation_Role_AssignmentID { get; set; }
            public Guid BOP_InterIdentityRelation_Role_Assignment_StatusID { get; set; }
            public Guid Relation_Role_Assignment_Status_GID { get; set; }
            public string Relation_Role_Assignment_Status_Name { get; set; }
            public List<BPT_BP_GBPID_1114_TrustLevel> TrustLevels { get; set; }
        }

        public class BPT_BP_GBPID_1114_TrustLevel
        {
            public Guid BOP_Relation_Roles_TrustLevelID { get; set; }
            public Guid Relation_Roles_TrustLevel_GID { get; set; }
            public string Relation_Roles_TrustLevel_Title { get; set; }
            public List<BPT_BP_GBPID_1114_TrustLevel_SharedWarehouse> TrustLevel_SharedWarehouses { get; set; }
        }

        public class BPT_BP_GBPID_1114_TrustLevel_SharedWarehouse
        {
            public Guid LOG_WRH_WarehouseID { get; set; }
            public string CoordinateCode { get; set; }
            public string Warehouse_Name { get; set; }
            public bool TransferCustomerOwnedStockInformation { get; set; }
            public bool TransferSelfOwnedStockInformation { get; set; }
        }

        public class BPT_BP_GBPID_1114_InternalRole
        {
            public Guid InternalNetworkRoleID { get; set; }
            public string Role_Name { get; set; }
        }
    }
}