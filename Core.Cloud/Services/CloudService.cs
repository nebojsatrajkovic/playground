using Core.Cloud.Database.ORM;
using Core.Cloud.Database.Query.Folder;
using Core.Cloud.Models.API.File;
using Core.Cloud.Models.API.Folder;
using Core.Shared.Configuration;
using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;
using Microsoft.AspNetCore.Http;

namespace Core.Cloud.Services
{
    public static class CloudService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(CloudService));
        static readonly string FileStoragePath = CORE_Configuration.Cloud.FileStoragePath;

        public static ResultOf InitializeCloudForUserAccount(CORE_DB_Connection connection, int userAccountID)
        {
            // initialize folders for newly created user account

            ResultOf returnValue;

            try
            {


                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to initialize cloud for user account: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        public static ResultOf<CLOUD_CreateFolder_Result> CreateFolder(CORE_DB_Connection connection, CLOUD_CreateFolder_Request parameter)
        {
            ResultOf<CLOUD_CreateFolder_Result> returnValue;

            try
            {
                // TODO decide how to organize folders on disk
                // TODO decide how to link the folders in database

                var existingFolders = Get_ExistingFolders_for_Name_and_ParentFolder.Invoke(connection.Connection, connection.Transaction, new P_GEFfNaPF
                {
                    FolderName = parameter.FolderName,
                    ParentFolderID = parameter.FolderParent_RefID > 0 ? parameter.FolderParent_RefID : null,
                    TenantID = parameter.TenantID,
                    AccountID = parameter.AccountID
                });

                if (existingFolders != null && existingFolders.Count > 0)
                {
                    return new ResultOf<CLOUD_CreateFolder_Result>(CORE_OperationStatus.FAILED, new CLOUD_CreateFolder_Result { IfFailed_FolderWithSameNameAlreadyExists = true }, $"Failed to create a folder '{parameter.FolderName}' since it already exists");
                }

                var folder = new cloud_folder.ORM
                {
                    folder_name = parameter.FolderName,
                    auth_account_refid = parameter.AccountID,
                    parent_folder_refid = parameter.FolderParent_RefID > 0 ? parameter.FolderParent_RefID : null,
                    tenant_refid = parameter.TenantID,
                    created_at = DateTime.Now,
                    modified_at = DateTime.Now
                };

                cloud_folder.Database.Save(connection, folder);

                // TODO create folder on disk as well

                var result = new CLOUD_CreateFolder_Result
                {
                    IsSuccess = true,
                    IfSuccess_FolderID = folder.cloud_folder_id
                };

                returnValue = new ResultOf<CLOUD_CreateFolder_Result>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to initialize cloud for user account: ", ex);

                returnValue = new ResultOf<CLOUD_CreateFolder_Result>(ex);
            }

            return returnValue;
        }

        public static ResultOf DeleteFolder(CORE_DB_Connection connection, CLOUD_DeleteFolder_Request parameter)
        {
            // delete folder and all it's files
            // delete the files on disk as well

            ResultOf returnValue;

            try
            {


                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to delete cloud folder: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        public static ResultOf UpdateFolderDetails(CORE_DB_Connection connection)
        {
            // rename folder, etc

            ResultOf returnValue;

            try
            {


                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to update cloud folder details: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        public static ResultOf GetFolderDetails(CORE_DB_Connection connection, int folderID)
        {
            // get folder details, sub-folders list, files list

            ResultOf returnValue;

            try
            {


                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to retrieve cloud folder details: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        public static async Task<ResultOf<CLOUD_UploadFile_Result>> UploadFile(CORE_DB_Connection connection, HttpContext context, CLOUD_UploadFile_Request parameter)
        {
            ResultOf<CLOUD_UploadFile_Result> returnValue;

            try
            {
                if (!context.Request.ContentLength.HasValue || context.Request.ContentLength <= 0)
                {
                    return new ResultOf<CLOUD_UploadFile_Result>(CORE_OperationStatus.FAILED, new CLOUD_UploadFile_Result { IfFailed_InvalidFileContent = true }, "Invalid file content.");
                }

                var fileName = context.Request.Headers["X-File-Name"].ToString();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return new ResultOf<CLOUD_UploadFile_Result>(CORE_OperationStatus.FAILED, new CLOUD_UploadFile_Result { IfFailed_InvalidFileName = true }, "File name is missing.");
                }

                var cloudFolder = cloud_folder.Database.Search(connection, new cloud_folder.QueryParameter
                {
                    auth_account_refid = parameter.AccountID,
                    tenant_refid = parameter.TenantID,
                    cloud_folder_id = parameter.FolderID
                }).FirstOrDefault();

                if (cloudFolder == null)
                {
                    return new ResultOf<CLOUD_UploadFile_Result>(CORE_OperationStatus.FAILED, new CLOUD_UploadFile_Result { IfFailed_FolderNotFoundInDatabase = true });
                }

                var storagePath = Path.Combine(FileStoragePath, parameter.TenantID.ToString(), parameter.AccountID.ToString(), cloudFolder.folder_path, cloudFolder.folder_name);

                if (!Directory.Exists(storagePath))
                {
                    return new ResultOf<CLOUD_UploadFile_Result>(CORE_OperationStatus.FAILED, new CLOUD_UploadFile_Result { IfFailed_FolderNotFoundOnCloud = true });
                }

                var safeFileName = Path.GetFileName(fileName); // avoid path traversal
                var filePath = Path.Combine(storagePath, safeFileName);

                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    await context.Request.Body.CopyToAsync(fileStream);
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while saving the file: ", ex);

                    throw;
                }

                var cloudFile = new cloud_file.ORM
                {
                    file_name = safeFileName,
                    file_path = filePath,
                    file_size_in_bytes = Convert.ToInt32(new FileInfo(filePath).Length),
                    folder_refid = cloudFolder.cloud_folder_id,
                    auth_account_refid = parameter.AccountID,
                    tenant_refid = parameter.TenantID,
                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow
                };

                cloud_file.Database.Save(connection, cloudFile);

                returnValue = new ResultOf<CLOUD_UploadFile_Result>(new CLOUD_UploadFile_Result { IsSuccess = true, IfSuccess_FileID = cloudFile.cloud_file_id });
            }
            catch (Exception ex)
            {
                logger.Error("Failed upload file: ", ex);

                returnValue = new ResultOf<CLOUD_UploadFile_Result>(ex);
            }

            return returnValue;
        }
    }
}