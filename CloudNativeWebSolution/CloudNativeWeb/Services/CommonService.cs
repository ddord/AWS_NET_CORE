using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using CloudNativeWeb.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWeb.Services
{
    public class CommonService
    {
        private Common.Database _Database;

        public CommonService()
        {
            _Database = new Common.Database();
        }
        
        public IEnumerable SelectAdminUser(string userId)
        {
            var result = _Database.ExecuteQuery(
                "SELECT * FROM UserInfo WHERE UserId = @UserId",
                new
                {
                    userId,
                },
                CommandType.Text);
            return result;
        }

        public int InsertUserInfo(UserInfo models)
        {
            string query = @"
                IF EXISTS(SELECT UserId FROM UserInfo WHERE UserId = @UserId)
		            BEGIN
			            SELECT -1
		            END
	            ELSE
		            BEGIN
		            INSERT INTO UserInfo
		            ( UserId,
                    Email,
                    Password,
		            CreateUser, 
		            CreateDate, 
		            UpdateUser, 
		            UpdateDate)
		            VALUES
		                (@UserId,		
		                @Email,
                        @Password,
                        @UserId,
		                GETDATE(),			 
		                @UserId,
		                GETDATE())

		            SELECT 1
		            END";

            var result = _Database.ExecuteNonQuery
                    (query,
                    new
                    {
                        UserId=models.UserId,
                        Email=models.Email,
                        Password=models.Password
                    },
                CommandType.Text);
            return result;
        }

        public async Task<IEnumerable<Movie>> SelectMovieList(int rating, string genre, string title)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                if (rating == 11 && genre.ToUpper() == "ALL" && title == null)
                {
                    var conditions = new List<ScanCondition>();

                    var models = await context.ScanAsync<Movie>(conditions).GetRemainingAsync();
                    return models;
                }
                else
                {
                    var request = new GetItemRequest
                    {
                        TableName = "Movie",
                        Key = new Dictionary<string, AttributeValue>
                        {
                            {"title", new AttributeValue{S=title} }
                        }
                    };

                    var response = await client.GetItemAsync(request);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (response.Item.Count > 0)
                        {
                            var result = response.Item;
                            //return result;
                        }
                    }
                }

            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (ResourceNotFoundException ex)
            {
                Console.WriteLine("The operation tried to access a nonexistent table or index.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
