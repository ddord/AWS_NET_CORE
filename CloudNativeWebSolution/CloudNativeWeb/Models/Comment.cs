using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWeb.Models
{
    public class Comment
    {
        [DynamoDBProperty("pk")]
        [DynamoDBHashKey("commentId")]
        public string CommentId { get; set; }

        [DynamoDBRangeKey("sk")]
        [DynamoDBProperty("title")]
        public string Title { get; set; }

        [DynamoDBProperty("commentContent")]
        public string CommentContent { get; set; }

        [DynamoDBProperty("rating")]
        public float Rating { get; set; }

        [DynamoDBProperty("updateDate")]
        public string UpdateDate { get; set; }

        [DynamoDBProperty("updateUser")]
        public string UpdateUser { get; set; }
    }
}
