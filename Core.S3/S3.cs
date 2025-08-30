using Amazon.Runtime;
using Amazon.S3;
using Core.S3.Model;
using Core.S3.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.S3
{
    public static class S3
    {
        public static IMvcBuilder UseCoreS3(this IMvcBuilder builder, CORE_S3_Configuration configuration)
        {
            S3Service.s3Client = new AmazonS3Client(new BasicAWSCredentials(configuration.S3_ACCESS_KEY_ID, configuration.S3_SECRET_ACCESS_KEY), new AmazonS3Config
            {
                ServiceURL = configuration.S3_SERVICE_URL
            });

            S3Service.s3BucketName = configuration.S3_BUCKET_NAME;

            return builder;
        }

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
            var configuration = new CORE_S3_Configuration
            {
                S3_ACCESS_KEY_ID = Environment.GetEnvironmentVariable(nameof(CORE_S3_Configuration.S3_ACCESS_KEY_ID)) ?? string.Empty,
                S3_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable(nameof(CORE_S3_Configuration.S3_SECRET_ACCESS_KEY)) ?? string.Empty,
                S3_SERVICE_URL = Environment.GetEnvironmentVariable(nameof(CORE_S3_Configuration.S3_SERVICE_URL)) ?? string.Empty,
                S3_BUCKET_NAME = Environment.GetEnvironmentVariable(nameof(CORE_S3_Configuration.S3_BUCKET_NAME)) ?? string.Empty
            };

            return UseCoreS3(builder, configuration);
        }
    }
}