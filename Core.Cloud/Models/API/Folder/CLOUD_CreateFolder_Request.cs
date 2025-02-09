namespace Core.Cloud.Models.API.Folder
{
    public class CLOUD_CreateFolder_Request
    {
        public int? FolderParent_RefID { get; set; }
        public string FolderName { get; set; } = null!;
        public int AccountID { get; set; }
        public int TenantID { get; set; }
    }
}