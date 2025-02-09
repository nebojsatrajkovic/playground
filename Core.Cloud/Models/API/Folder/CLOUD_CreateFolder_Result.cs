namespace Core.Cloud.Models.API.Folder
{
    public class CLOUD_CreateFolder_Result
    {
        public bool IsSuccess { get; set; }
        public int IfSuccess_FolderID { get; set; }

        public bool IfFailed_FolderWithSameNameAlreadyExists { get; set; }
    }
}