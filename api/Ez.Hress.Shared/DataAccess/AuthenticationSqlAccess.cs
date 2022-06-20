using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    public class AuthenticationSqlAccess : IAuthenticationDataAccess
    {
        private readonly ILogger<AuthenticationSqlAccess> _log;
        private readonly string _connectionString;

        public AuthenticationSqlAccess(DbConnectionInfo connectionInfo, ILogger<AuthenticationSqlAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }
                

        public async Task<int> GetUserID(string username, string hashedPassword)
        {
            var sql = "SELECT Id, ApiPassword FROM adm_User WHERE Username = @username";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var apiPassword = reader.GetString(1);
                            if (apiPassword == hashedPassword)
                            {
                                return reader.GetInt32(0);
                            }
                        }
                        
                    }
                }
            }

            return -1;
        }

        public async Task SaveLoginInformation(int userId, string ipAddress)
        {
            var sql = @"UPDATE adm_User
                SET LastLoggedIn = GetDate(),
                    LastLoggedInBy = @ipAddress,
                    LoginCount = LoginCount + 1,
                    IsOnline = 1
            WHERE Id = @userID";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userID", userId);
                    command.Parameters.AddWithValue("@ipAddress", ipAddress);

                    _ = await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
