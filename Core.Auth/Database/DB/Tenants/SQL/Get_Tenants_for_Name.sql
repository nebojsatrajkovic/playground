
		SELECT
		auth_tenant.auth_tenant_id,
		auth_tenant.tenant_name
		FROM auth_tenant
		WHERE auth_tenant.is_deleted = 0 And
		Lower(auth_tenant.tenant_name) like Concat('%', Lower(@Name), '%')
	