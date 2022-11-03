using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Ez.Hress.UserProfile.Entities;
using Ez.Hress.UserProfile.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UserProfile.DataAccess
{
    public class UserProfileSqlDataAccess : IUserProfileDataAccess
    {
        private readonly ILogger<UserProfileSqlDataAccess> _log;
        private readonly string _connectionString;

        public UserProfileSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<UserProfileSqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<IList<Transaction>> GetTransactions(int userID)
        {
            const string sql = @"SELECT	debt.ID, debt.UserID, debt.Name, debt.Amount, debt.Inserted, debt.InsertedBy
                                FROM	upf_Debt debt
                                WHERE	debt.UserID = @userID AND debt.Deleted IS NULL
                                ORDER BY debt.Inserted";

            var list = new List<Transaction>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userID", userID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var entity = new Transaction()
                            {
                                ID = SqlHelper.GetInt(reader, "ID"),
                                UserID = SqlHelper.GetInt(reader, "UserID"),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Amount = SqlHelper.GetInt(reader, "Amount"),
                                Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
                                InsertedBy = SqlHelper.GetInt(reader, "InsertedBy")
                            };

                            list.Add(entity);
                        }
                    }
                }
            }

            return list;
        }
    }
}
