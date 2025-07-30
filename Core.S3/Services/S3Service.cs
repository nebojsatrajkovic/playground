using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace Core.S3.Services
{
    public static class S3Service
    {
        internal static IAmazonS3 s3Client;
        internal static string s3BucketName;

        public static async Task Upload(Stream stream, string keyPrefix, string keyFragment)
        {
            EnsureBucketExists();

            var key = $"{keyPrefix}/{keyFragment}";

            var request = new PutObjectRequest
            {
                BucketName = s3BucketName,
                Key = key,
                InputStream = stream,
                DisablePayloadSigning = true
            };

            await s3Client.PutObjectAsync(request);
        }

        public static async Task Upload(IFormFile file, string keyPrefix, string keyFragment)
        {
            EnsureBucketExists();

            var key = $"{keyPrefix}/{keyFragment}";

            var request = new PutObjectRequest
            {
                BucketName = s3BucketName,
                Key = key,
                InputStream = file.OpenReadStream(),
                DisablePayloadSigning = true
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            await s3Client.PutObjectAsync(request);
        }

        public static async Task<ListObjectsV2Response> List(string keyPrefix)
        {
            EnsureBucketExists();

            var request = new ListObjectsV2Request
            {
                BucketName = s3BucketName,
                Prefix = keyPrefix
            };

            return await s3Client.ListObjectsV2Async(request);
        }

        public static async void Delete(string keyPrefix, string keyFragment)
        {
            EnsureBucketExists();

            var key = $"{keyPrefix}/{keyFragment}";

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = s3BucketName,
                Key = key
            };

            var res = await s3Client.DeleteObjectAsync(deleteObjectRequest);
        }

        public static async Task<long?> CalculateStorage(string keyPrefix, string keyFragment)
        {
            EnsureBucketExists();

            var key = $"{keyPrefix}/{keyFragment}";

            var data = await s3Client.ListObjectsAsync(s3BucketName, key);

            var size = data.S3Objects.Sum(x => x.Size); // in bytes

            return size;
        }

        public static async void DeleteAllObjects(string keyPrefix, string keyFragment)
        {
            var key = $"{keyPrefix}/{keyFragment}";

            var data = await s3Client.ListObjectsAsync(s3BucketName, key);

            var keys = data.S3Objects.Select(x => x.Key).ToList();

            foreach (var item in keys)
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = item
                };

                var res = await s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
        }

        static async void EnsureBucketExists()
        {
            await s3Client.EnsureBucketExistsAsync(s3BucketName);
        }
    }
}