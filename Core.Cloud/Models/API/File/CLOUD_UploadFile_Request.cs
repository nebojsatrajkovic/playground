namespace Core.Cloud.Models.API.File
{
    public class CLOUD_UploadFile_Request
    {
        public int FolderID { get; set; }
        public int AccountID { get; set; }
        public int TenantID { get; set; }
    }
}