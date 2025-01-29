namespace Core.Cloud.Models
{
    public class FileMetadata
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid UserAccountID { get; set; }
    }
}