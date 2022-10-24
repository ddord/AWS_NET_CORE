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
        
        public IEnumerable SelectAdminUser(string userId, string password)
        {
            var result = _Database.ExecuteQuery(
                "SELECT * FROM UserInfo WHERE UserId = @UserId AND Password = @Password",
                new
                {
                    userId,
                    password
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

        public async Task<IEnumerable<Movie>> SelectMovieList(double rating, string genre, string title)
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
                    var conditions = new List<ScanCondition>();
                    if (rating != 11)
                        conditions.Add(new ScanCondition("Rating", ScanOperator.GreaterThanOrEqual, rating));

                    if (genre.ToUpper() == "ALL")
                        conditions.Add(new ScanCondition("Genre", ScanOperator.Equal, genre));

                    if (!string.IsNullOrEmpty(title))
                        conditions.Add(new ScanCondition("Title", ScanOperator.Equal, title));
                    var models = await context.ScanAsync<Movie>(conditions, new DynamoDBOperationConfig() { }).GetRemainingAsync();
                    return models;
                }

            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<Movie>> SelectMovieItem(string title)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                var conditions = new List<ScanCondition>();

                conditions.Add(new ScanCondition("Title", ScanOperator.Equal, title));
                var models = await context.ScanAsync<Movie>(conditions, new DynamoDBOperationConfig() { }).GetRemainingAsync();
                return models;
            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<int> InsertMovieItem(Movie models)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                Movie updateMovie = await context.LoadAsync<Movie>(models.Title);

                if (updateMovie != null)
                {
                    models.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    models.UpdateUser = Common.UserInfo.Current.DisplayName;
                    await context.SaveAsync(models);
                    return 1;
                }
                else
                {
                    return 0;
                }
                
            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        public async Task<int> UpdateMovieItem(Movie models)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                //Movie updateMovie = await context.LoadAsync<Movie>(models.Title);
                models.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                models.UpdateUser = Common.UserInfo.Current.DisplayName;
                await context.SaveAsync(models);
                return 1;
            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        public async Task<IEnumerable<Comment>> SelectCommentList(string title)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                var conditions = new List<ScanCondition>();

                if (!string.IsNullOrEmpty(title))
                    conditions.Add(new ScanCondition("Title", ScanOperator.Equal, title));
                var models = await context.ScanAsync<Comment>(conditions, new DynamoDBOperationConfig() { }).GetRemainingAsync();
                return models;
            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<int> InsertComment(Comment models)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                Comment updateModel = await context.LoadAsync<Comment>(models.CommentId, models.Title);

                if (updateModel == null)
                {
                    models.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    await context.SaveAsync(models);

                    var conditions = new List<ScanCondition>();

                    if (!string.IsNullOrEmpty(models.Title))
                        conditions.Add(new ScanCondition("Title", ScanOperator.Equal, models.Title));
                    var selectComment = await context.ScanAsync<Comment>(conditions, new DynamoDBOperationConfig() { }).GetRemainingAsync();
                    if (selectComment != null)
                    {
                        double sum = 0.0;
                        foreach (var item in selectComment)
                        {
                            sum += item.Rating;
                        }

                        double rating = Math.Round((sum / selectComment.Count), 1);
                        Movie updateMovie = await context.LoadAsync<Movie>(models.Title);
                        updateMovie.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                        updateMovie.UpdateUser = models.UpdateUser;
                        updateMovie.Rating = rating;
                        await context.SaveAsync(updateMovie);
                    }

                    return 1;
                }
                else
                {
                    return 0;
                }

            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        public async Task<int> UpdateComment(Comment models)
        {
            var client = Common.CloudHelper.dyClient;

            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                Comment updateModel = await context.LoadAsync<Comment>(models.CommentId);

                if (updateModel != null)
                {
                    updateModel.CommentContent = models.CommentContent;
                    updateModel.Rating = models.Rating;
                    updateModel.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    updateModel.UpdateUser = Common.UserInfo.Current.DisplayName;
                    await context.SaveAsync(updateModel);

                    var conditions = new List<ScanCondition>();

                    if (!string.IsNullOrEmpty(models.Title))
                        conditions.Add(new ScanCondition("Title", ScanOperator.Equal, models.Title));
                    var selectComment = await context.ScanAsync<Comment>(conditions, new DynamoDBOperationConfig() { }).GetRemainingAsync();
                    if (selectComment != null)
                    {
                        double sum = 0.0;
                        foreach (var item in selectComment)
                        {
                            sum += item.Rating;
                        }

                        double rating = Math.Round((sum / selectComment.Count), 1);
                        Movie updateMovie = await context.LoadAsync<Movie>(models.Title);
                        updateMovie.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                        updateMovie.UpdateUser = models.UpdateUser;
                        updateMovie.Rating = rating;
                        await context.SaveAsync(updateMovie);
                    }

                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine("An error occurred on the server side " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }
    }
}
