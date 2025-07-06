
		SELECT
		auth_tenant.auth_tenant_id,
		auth_tenant.tenant_name
		FROM auth_account
		INNER JOIN auth_tenant ON auth_tenant.auth_tenant_id = auth_account.tenant_refid And
		auth_tenant.is_deleted = 0
		WHERE Lower(auth_account.email) = Lower(@Email)
	