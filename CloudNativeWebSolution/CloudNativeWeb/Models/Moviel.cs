using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWeb.Models
{
    public class Movie
    {
        [DynamoDBProperty("pk")]
        [DynamoDBHashKey("title")]
        public string Title { get; set; }

        [DynamoDBRangeKey(AttributeName = "sk")]
        [DynamoDBProperty("genre")]
        public string Genre { get; set; }

        [DynamoDBProperty("releasTime")]
        public string ReleasTime { get; set; }

        [DynamoDBProperty("director")]
        public string Director { get; set; }

        [DynamoDBProperty("rating")]
        public float Rating { get; set; }

        [DynamoDBProperty("updateDate")]
        public string UpdateDate { get; set; }

        [DynamoDBProperty("updateUser")]
        public string UpdateUser { get; set; }
    }
}
