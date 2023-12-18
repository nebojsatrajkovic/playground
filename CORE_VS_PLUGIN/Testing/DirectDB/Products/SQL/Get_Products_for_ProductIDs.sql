
		Select
		cmn_pro_products.CMN_PRO_ProductID,
		cmn_pro_products.Product_UID,
		cmn_pro_products.Product_Name_DictID,
		cmn_pro_products.Product_Number,
		cmn_pro_product_attached_documents.Document_RefID,
		cmn_pro_product_variants.CMN_PRO_Product_VariantID,
		cmn_pro_product_variants.VariantName_DictID,
		cmn_pro_product_variants.IsStandardProductVariant,
		cmn_pro_product_variants.IsBlockedForOrdering,
		cmn_pro_product_variants.IsVariantKeptInStock,
		cmn_pro_product_variants.IsBlockedWhenNotOnStock,
		cmn_pro_product_variants.OrderSequenceNumber,
		cmn_pro_cus_customizations.CMN_PRO_CUS_CustomizationID,
		cmn_pro_cus_customizations.Customization_Name_DictID,
		cmn_pro_cus_customizations.OrderSequence As C_OrderSequence,
		cmn_pro_cus_customization_variants.CMN_PRO_CUS_Customization_VariantID,
		cmn_pro_cus_customization_variants.CustomizationVariant_Name_DictID,
		cmn_pro_cus_customization_variants.OrderSequence As CVariant_OrderSequence,
		cmn_pro_product_attributes.MinimumOrderQuantity,
		cmn_pro_product_attributes.OrderQuantityInterval,
		pro_m2m_cut_cutrevisions_assignedproducts.CutRevision_RefID
		From
		cmn_pro_products Inner Join
		cmn_pro_product_variants On cmn_pro_products.CMN_PRO_ProductID = cmn_pro_product_variants.CMN_PRO_Product_RefID
		And cmn_pro_product_variants.IsDeleted = 0
		And cmn_pro_product_variants.Tenant_RefID = @TenantID
		And cmn_pro_product_variants.IsObsolete = 0 Left Join
		cmn_pro_cus_customizations On cmn_pro_products.CMN_PRO_ProductID = cmn_pro_cus_customizations.Product_RefID
		And cmn_pro_cus_customizations.IsDeleted = 0
		And cmn_pro_cus_customizations.Tenant_RefID = @TenantID
		And cmn_pro_cus_customizations.IsObsolete = 0 Left Join
		cmn_pro_cus_customization_variants On cmn_pro_cus_customizations.CMN_PRO_CUS_CustomizationID =
		cmn_pro_cus_customization_variants.Customization_RefID
		And cmn_pro_cus_customization_variants.IsDeleted = 0
		And cmn_pro_cus_customization_variants.Tenant_RefID = @TenantID
		And cmn_pro_cus_customization_variants.IsObsolete = 0 Left Join
		cmn_pro_product_attachments On cmn_pro_products.CMN_PRO_ProductID = cmn_pro_product_attachments.Product_RefID
		And cmn_pro_product_attachments.IsDeleted = 0
		And cmn_pro_product_attachments.Tenant_RefID = @TenantID
		And cmn_pro_product_attachments.IsDisplayedInGallery = 1 Left Join
		cmn_pro_product_attached_documents On cmn_pro_product_attachments.CMN_PRO_Product_AttachmentID =
		cmn_pro_product_attached_documents.Product_Attachment_RefID
		And cmn_pro_product_attached_documents.IsDeleted = 0
		And cmn_pro_product_attached_documents.Tenant_RefID = @TenantID
		And cmn_pro_product_attached_documents.IsBoundToVariant = 0
		And cmn_pro_product_attached_documents.IsBoundToDimensionValue = 0
		And cmn_pro_product_attached_documents.IsLanguageSpecific = 0 Left Join
		cmn_pro_product_attributes On cmn_pro_product_variants.CMN_PRO_Product_VariantID =
		cmn_pro_product_attributes.Variant_RefID
		And cmn_pro_product_attributes.IsDeleted = 0
		And cmn_pro_product_attributes.Tenant_RefID = @TenantID LEFT Join
		pro_m2m_cut_cutrevisions_assignedproducts On cmn_pro_products.CMN_PRO_ProductID =
		pro_m2m_cut_cutrevisions_assignedproducts.CMN_PRO_Products_RefID and
		pro_m2m_cut_cutrevisions_assignedproducts.IsDeleted = 0 And
		pro_m2m_cut_cutrevisions_assignedproducts.Tenant_RefID = @TenantID
		Where
		cmn_pro_products.IsDeleted = 0 And
		cmn_pro_products.Tenant_RefID = @TenantID And
		cmn_pro_products.IsObsolete = 0 And
		((@IsUseIDs = 1 And
		cmn_pro_products.CMN_PRO_ProductID = @ProductIDs) Or
		(@IsUseUIDs = 1 And
		cmn_pro_products.Product_UID = @ProductIDs))
	