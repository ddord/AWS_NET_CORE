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
    }
}
