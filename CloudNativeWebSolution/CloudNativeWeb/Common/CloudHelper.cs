using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWeb.Common
{
    public class CloudHelper
    {
        public static AmazonS3Client s3Client = GetAmazonS3Client();
        public static AmazonDynamoDBClient dyClient = GetAmazonDynamoDBClient();

        private static AmazonS3Client GetAmazonS3Client()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("AppSettings.json");
            var accessKeyID = builder.Build().GetSection("AWSCredentials").GetSection("AccesskeyID").Value;
            var secretKey = builder.Build().GetSection("AWSCredentials").GetSection("Secretaccesskey").Value;
            BasicAWSCredentials credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            s3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
            return s3Client;
        }

        public static string GetRDSConnectionString()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("AppSettings.json");
            EncryptionHelper helper = new EncryptionHelper();
            var connectionString = helper.Decrypt(builder.Build().GetSection("ConnectionStrings").GetSection("Main").Value);
            return connectionString;
        }

        private static AmazonDynamoDBClient GetAmazonDynamoDBClient()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("AppSettings.json");
            var accessKeyID = builder.Build().GetSection("AWSCredentials").GetSection("AccesskeyID").Value;
            var secretKey = builder.Build().GetSection("AWSCredentials").GetSection("Secretaccesskey").Value;
            BasicAWSCredentials credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            dyClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            return dyClient;
        }

        //public static string GetRDSConnectionString()
        //{
        //    var appConfig = ConfigurationManager;

        //    string dbname = appConfig["RDS_DB_NAME"];

        //    if (string.IsNullOrEmpty(dbname)) return null;

        //    string username = appConfig["RDS_USERNAME"];
        //    string password = appConfig["RDS_PASSWORD"];
        //    string hostname = appConfig["RDS_HOSTNAME"];
        //    string port = appConfig["RDS_PORT"];

        //    return "Data Source=" + hostname + ";Initial Catalog=" + dbname + ";User ID=" + username + ";Password=" + password + ";";
        //}
    }
}
