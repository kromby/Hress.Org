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
    public class UserSqlDataAccess : IUserDataAccess
    {
        private readonly ILogger<UserSqlDataAccess> _log;
        private readonly string _connectionString;

        public UserSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<UserSqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }
        
        public async Task<UserBasicEntity> GetUser(int id)
        {
            var sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, uimg.ImageId
                        FROM adm_User usr
                        LEFT JOIN upf_Image uimg ON usr.Id = uimg.UserId AND uimg.TypeId = 14
                        WHERE usr.Id = @userID";

            UserBasicEntity user = new();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userID", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.ID = reader.GetInt32(reader.GetOrdinal("Id"));
                            user.Name = reader.GetString(reader.GetOrdinal("Username"));
                            user.Username = reader.GetString(reader.GetOrdinal("Username"));
                            user.Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted"));

                            if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                            {
                                user.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("ImageId"));
                            }
                        }
                    }
                }
            }

            return user;
        }
    }
}
