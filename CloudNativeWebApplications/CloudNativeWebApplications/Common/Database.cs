using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNativeWebApplications.Common
{
    public class Database
    {
        string ConnectionString = CommonHelper.GetRDSConnectionString();

        public IEnumerable ExecuteQuery(string query, object param = null, CommandType? commandType = null, DbTransaction transaction = null, int? commandTimeout = null)
        {
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<dynamic>(query, param: param, commandType: commandType, transaction: transaction, commandTimeout: commandTimeout);
                return result;
            }
        }

        public List<IEnumerable> ExecuteQueryMultiple(string query, object param = null, CommandType? commandType = null, DbTransaction transaction = null, int? commandTimeout = null)
        {
            List<IEnumerable> results = new List<IEnumerable>();
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                SqlMapper.GridReader reader = conn.QueryMultiple(query, param: param, commandType: commandType, transaction: transaction, commandTimeout: commandTimeout);
                while (!reader.IsConsumed)
                {
                    IEnumerable result = reader.Read().ToList();
                    results.Add(result);
                }
                reader.Dispose();
                return results;
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, object param = null, CommandType? commandType = null, DbTransaction transaction = null, int? commandTimeout = null)
        {
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                IEnumerable<T> result = conn.Query<T>(query, param: param, commandType: commandType, transaction: transaction, commandTimeout: commandTimeout);
                return result;
            }
        }

        public int ExecuteNonQuery(string query, object param = null, CommandType? commandType = null, DbTransaction transaction = null, int? commandTimeout = null)
        {
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(query, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
                return result;
            }
        }

        public object ExecuteScalar(string query, object param = null, CommandType? commandType = null, DbTransaction transaction = null, int? commandTimeout = null)
        {
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                var result = conn.ExecuteScalar(query, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
                return result;
            }
        }
    }
}
