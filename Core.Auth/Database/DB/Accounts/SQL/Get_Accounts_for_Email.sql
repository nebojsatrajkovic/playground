
		SELECT
		auth_account.auth_account_id
		FROM auth_account
		WHERE
		auth_account.is_deleted = 0 And
		auth_account.tenant_refid = @TenantID And
		Lower(auth_account.email) = Lower(@Email)
	