namespace Core.Cloud.Models.API.File
{
    public class CLOUD_UploadFile_Result
    {
        public bool IsSuccess { get; set; }
        public int IfSuccess_FileID { get; set; }

        public bool IfFailed_FolderNotFoundInDatabase { get; set; }
        public bool IfFailed_FolderNotFoundOnCloud { get; set; }
        public bool IfFailed_InvalidFileName { get; set; }
        public bool IfFailed_InvalidFileContent { get; set; }
    }
}