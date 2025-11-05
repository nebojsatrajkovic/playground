
		SELECT
		auth_right.auth_right_id,
		auth_right.right_code,
		auth_right.right_gid
		FROM
		auth_account_2_right
		INNER JOIN auth_right
		ON
		auth_account_2_right.right_refid = auth_right.auth_right_id
		AND auth_right.is_deleted = 0 AND
		auth_right.tenant_refid = @TenantID

		WHERE
		auth_account_2_right.is_deleted = 0 AND
		auth_account_2_right.tenant_refid = @TenantID And
		auth_account_2_right.account_refid = @AccountID
	