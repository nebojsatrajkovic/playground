namespace Core.Cloud.Models
{
    public class FileMetadata
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid UserAccountID { get; set; }
    }
}