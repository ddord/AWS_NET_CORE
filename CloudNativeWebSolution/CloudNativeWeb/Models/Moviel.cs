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

        [DynamoDBProperty("genre")]
        public string Genre { get; set; }

        [DynamoDBProperty("releaseTime")]
        public string ReleaseTime { get; set; }

        [DynamoDBProperty("director")]
        public string Director { get; set; }

        [DynamoDBProperty("rating")]
        public double Rating { get; set; }

        [DynamoDBProperty("updateDate")]
        public string UpdateDate { get; set; }

        [DynamoDBProperty("updateUser")]
        public string UpdateUser { get; set; }
    }
}
