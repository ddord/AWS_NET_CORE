using CloudNativeWebApplications.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWebApplications.Services
{
    public class CommonService
    {
        private Database _Database;

        public CommonService()
        {
            _Database = new Database();
        }

        public IEnumerable SelectAdminUser(string userId)
        {
            var result = _Database.ExecuteQuery(
                "SELECT * FROM [dbo].[UserInfo] WHERE [UserId] = @UserId",
                new
                {
                    userId,
                },
                CommandType.StoredProcedure);
            return result;
        }
    }
}
