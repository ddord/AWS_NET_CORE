using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWeb.Models
{
    public class MovieFile
    {
        [DynamoDBProperty("pk")]
        [DynamoDBHashKey("title")]
        public string Title { get; set; }

        [DynamoDBProperty("bucketName")]
        public string BucketName { get; set; }

        [DynamoDBProperty("fileName")]
        public string FileName { get; set; }

        [DynamoDBProperty("fileDisplayName")]
        public string FileDisplayName { get; set; }

        [DynamoDBProperty("updateDate")]
        public string UpdateDate { get; set; }

        [DynamoDBProperty("updateUser")]
        public string UpdateUser { get; set; }
    }
}
