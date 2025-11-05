using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Auth.Models.Authorization;
using Core.Shared.Models;
using Core.Shared.Utils.ThreadsafeCollections;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;

namespace Core.Auth.Services
{
    public static class AuthorizationService
    {
        static readonly CORE_TS_Dictionary<int, CachedRight> CachedRights;

        static readonly ILog logger = LogManager.GetLogger(typeof(AuthorizationService));

        static AuthorizationService()
        {
            CachedRights = new(TimeSpan.FromMinutes(30));
        }

        public async static Task<ResultOf> ValidateRequiredRightsAsync(CORE_DB_Connection connection, List<string> requiredRights)
        {
            try
            {
                if (requiredRights == null || requiredRights.Count == 0) return ResultOf.Success;

                // 1. check from cache
                if (CachedRights.TryGetValue(connection.AccountID, out var cachedRights) && cachedRights != null)
                {
                    if (cachedRights.Rights == null || cachedRights.Rights.Count == 0) return ResultOf.Unauthorized;

                    cachedRights.LastAccessedAt_UTC = DateTime.UtcNow;

                    foreach (var requiredRight in requiredRights)
                    {
                        if (!cachedRights.Rights.Contains(requiredRight)) return ResultOf.Unauthorized;
                    }

                    return ResultOf.Success;
                }

                // 2. check from database and update cache

                var dbAccountRights = await Get_Account_Rights.InvokeAsync(connection.Connection, connection.Transaction, new P_GAR { AccountID = connection.AccountID, TenantID = connection.TenantID });

                if (dbAccountRights != null && dbAccountRights.Count > 0)
                {
                    cachedRights = new CachedRight
                    {
                        LastAccessedAt_UTC = DateTime.UtcNow,
                        Rights = dbAccountRights.Select(x => x.right_code).ToHashSet()
                    };

                    CachedRights.AddOrUpdate(connection.AccountID, cachedRights);

                    foreach (var requiredRight in requiredRights)
                    {
                        if (!cachedRights.Rights.Contains(requiredRight)) return ResultOf.Unauthorized;
                    }

                    return ResultOf.Success;
                }

                return ResultOf.Unauthorized;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to validate required rights: ", ex);

                return new ResultOf(ex, "Failed to validate required rights.");
            }
        }

        public async static Task<ResultOf> ImportRights(CORE_DB_Connection connection, AUTH_ImportRights_Request parameter)
        {
            try
            {
                if (parameter.RightGroups == null || parameter.RightGroups.Count == 0) return ResultOf.Success;

                if (!parameter.IsMatchRightsBy_GID && !parameter.IsMatchRightsBy_Code)
                {
                    return new ResultOf(CORE_OperationStatus.FAILED, "Right matching must be selected. Either Guid GIDs or string Code.");
                }

                throw new NotImplementedException();

                var dbGroups = await auth_rightgroup.Database.SearchAsync(connection, new auth_rightgroup.QueryParameter
                {
                    is_deleted = false,
                    tenant_refid = connection.TenantID
                });

                var dbGroupsToRemove = dbGroups.Where(x => parameter.RightGroups.Any(rg => string.Equals(rg.RightGroup_Name, x.rightgroup_name, StringComparison.OrdinalIgnoreCase))).ToList();

                if (dbGroupsToRemove.Count > 0)
                {
                    dbGroupsToRemove.ForEach(async x =>
                    {
                        await auth_rightgroup.Database.SoftDeleteAsync(connection, x);

                        var dbBoundRights = await auth_right.Database.SearchAsync(connection, new auth_right.QueryParameter
                        {
                            is_deleted = false,
                            tenant_refid = connection.TenantID,
                            rightgroup_refid = x.auth_rightgroup_id
                        });

                        await auth_right.Database.SoftDeleteAsync(connection, new auth_right.QueryParameter
                        {
                            is_deleted = false,
                            tenant_refid = connection.TenantID,
                            rightgroup_refid = x.auth_rightgroup_id
                        });

                        dbBoundRights.ForEach(async r =>
                        {
                            await auth_account_2_right.Database.SoftDeleteAsync(connection, new auth_account_2_right.QueryParameter
                            {
                                is_deleted = false,
                                tenant_refid = connection.TenantID,
                                right_refid = r.auth_right_id
                            });
                        });
                    });
                }

                foreach (var rightGroup in parameter.RightGroups)
                {
                    var dbGroup = dbGroups.FirstOrDefault(x => string.Equals(x.rightgroup_name, rightGroup.RightGroup_Name, StringComparison.OrdinalIgnoreCase));

                    if (dbGroup == null)
                    {
                        // TODO manage parent/child structure


                    }


                }

                var dbRights = await auth_right.Database.SearchAsync(connection, new auth_right.QueryParameter
                {
                    is_deleted = false,
                    tenant_refid = connection.TenantID
                });

                return ResultOf.Success;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to import rights: ", ex);

                return new ResultOf(ex, "Failed to import rights.");
            }
        }
    }
}