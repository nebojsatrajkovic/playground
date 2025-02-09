
		select cloud_folder.cloud_folder_id
		FROM cloud_folder
		WHERE cloud_folder.parent_folder_refid = @ParentFolderID And
		Lower(cloud_folder.folder_name) = Lower(@FolderName) And
		cloud_folder.is_deleted = 0 And
		cloud_folder.auth_account_refid = @AccountID and
		cloud_folder.tenant_refid = @TenantID
	