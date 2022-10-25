using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CloudNativeWeb.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
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
                        UserId = models.UserId,
                        Email = models.Email,
                        Password = models.Password
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

                    if (genre.ToUpper() != "ALL")
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

                if (updateMovie == null)
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
                Comment updateModel = await context.LoadAsync<Comment>(models.CommentId, models.Title);

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

        public async Task<MovieFile> UploadFileAsync(List<IFormFile> files, string title)
        {
            var s3Client = Common.CloudHelper.s3Client;
            var client = Common.CloudHelper.dyClient;
            try
            {

                if (files is null || files.Count <= 0)
                    throw new Exception("file is required to upload");

                string fileGuid = Convert.ToString(Guid.NewGuid());

                using (var newMemoryStream = new MemoryStream())
                {
                    files[0].CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = fileGuid,
                        BucketName = "user01-bucket02",
                        ContentType = files[0].ContentType
                    };

                    var fileTransferUtility = new TransferUtility(s3Client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }

                DynamoDBContext context = new DynamoDBContext(client);

                MovieFile movieFile = new MovieFile();
                movieFile.Title = title;
                movieFile.FileName = fileGuid;
                movieFile.FileDisplayName = files[0].FileName;
                movieFile.BucketName = "user01-bucket02";
                movieFile.UpdateDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                movieFile.UpdateUser = Common.UserInfo.Current.DisplayName;

                await context.SaveAsync(movieFile);

                return movieFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            var s3Client = Common.CloudHelper.s3Client;
            var client = Common.CloudHelper.dyClient;
            MemoryStream ms = null;

            try
            {
                GetObjectRequest getObjectRequest = new GetObjectRequest
                {
                    BucketName = "user01-bucket02",
                    Key = fileName
                };

                using (var response = await s3Client.GetObjectAsync(getObjectRequest))
                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using (ms = new MemoryStream())
                        {
                            await response.ResponseStream.CopyToAsync(ms);
                        }
                    }
                }

                if (ms is null || ms.ToArray().Length < 1)
                    throw new FileNotFoundException(string.Format("The document '{0}' is not found", fileName));

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<MovieFile> SelectFileAsync(string title)
        {
            var client = Common.CloudHelper.dyClient;
            try
            {
                DynamoDBContext context = new DynamoDBContext(client);
                MovieFile models = await context.LoadAsync<MovieFile>(title);
                return models;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
