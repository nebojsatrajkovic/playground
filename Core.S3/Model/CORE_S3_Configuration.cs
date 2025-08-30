namespace Core.S3.Model
{
    public class CORE_S3_Configuration
    {
        public string S3_ACCESS_KEY_ID { get; set; } = null!;
        public string S3_SECRET_ACCESS_KEY { get; set; } = null!;
        public string S3_SERVICE_URL { get; set; } = null!;
        public string S3_BUCKET_NAME { get; set; } = null!;
    }
}