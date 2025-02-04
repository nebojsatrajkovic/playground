
		SELECT auth_session.auth_session_id, auth_session.session_token FROM auth_session WHERE auth_session.is_deleted = 0 And auth_session.tenant_id = @TenantID And auth_session.valid_to < @DateThreshold
	