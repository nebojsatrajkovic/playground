using Core.Cloud.Models;
using Core.Shared.Models;
using log4net;
using Microsoft.AspNetCore.Http;

namespace Core.Cloud.Services
{
    public static class CloudService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(CloudService));
        static readonly string FileStoragePath = "FileStorage";

        public static void InitializeCloudForUserAccount(Guid userAccountID)
        {
            // initialize folders for newly created user account
        }

        public static void CreateFolder()
        {
            // create folder
            // * if name already exists -> reject
        }

        public static void DeleteFolder()
        {
            // delete folder and all it's files
        }

        public static void UpdateFolderDetails()
        {
            // rename folder, etc
        }

        public static void GetFolderDetails(Guid folderID)
        {
            // get folder details, sub-folders list, files list
        }

        public static async Task<ResultOf<FileMetadata>> UploadFile(HttpContext context, Guid folderID, Guid userAccountID)
        {
            ResultOf<FileMetadata> returnValue;

            try
            {
                if (!context.Request.ContentLength.HasValue || context.Request.ContentLength <= 0)
                {
                    return new ResultOf<FileMetadata>(CORE_OperationStatus.FAILED, "Invalid file content.");
                }

                var fileName = context.Request.Headers["X-File-Name"].ToString();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return new ResultOf<FileMetadata>(CORE_OperationStatus.FAILED, "File name is missing.");
                }

                var folderName = string.Empty; // TODO get folder by ID

                var storagePath = Path.Combine(FileStoragePath, userAccountID.ToString(), folderName);

                Directory.CreateDirectory(storagePath);

                var safeFileName = Path.GetFileName(fileName); // avoid path traversal
                var filePath = Path.Combine(storagePath, safeFileName);

                try
                {
                    // TODO take care of files being overwritten - potentially ok

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        await context.Request.Body.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while saving the file: ", ex);

                    throw;
                }

                var metadata = new FileMetadata
                {
                    FileID = Guid.NewGuid(),
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = new FileInfo(filePath).Length,
                    UploadedAt = DateTime.UtcNow,
                    UserAccountID = userAccountID
                };

                // TODO store metadata

                returnValue = new ResultOf<FileMetadata>(metadata);
            }
            catch (Exception ex)
            {
                logger.Error("Failed upload file: ", ex);

                returnValue = new ResultOf<FileMetadata>(ex);
            }
            
            return returnValue;
        }
    }
}