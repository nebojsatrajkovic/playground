using Amazon.Runtime;
using Amazon.S3;
using Core.S3.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.S3
{
    public static class S3
    {
        /// <summary>
        /// In order to use this S3 client developer must specify following environment variables:
        /// a. S3_ACCESS_KEY_ID
        /// b. S3_SECRET_ACCESS_KEY
        /// c. S3_SERVICE_URL
        /// d. S3_BUCKET_NAME
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder UseCoreS3(this IMvcBuilder builder)
        {
            var S3_ACCESS_KEY_ID = Environment.GetEnvironmentVariable("S3_ACCESS_KEY_ID");
            var S3_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable("S3_SECRET_ACCESS_KEY");
            var S3_SERVICE_URL = Environment.GetEnvironmentVariable("S3_SERVICE_URL");
            var S3_BUCKET_NAME = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? string.Empty;

            S3Service.s3Client = new AmazonS3Client(new BasicAWSCredentials(S3_ACCESS_KEY_ID, S3_SECRET_ACCESS_KEY), new AmazonS3Config
            {
                ServiceURL = S3_SERVICE_URL
            });

            S3Service.s3BucketName = S3_BUCKET_NAME;

            return builder;
        }
    }
}