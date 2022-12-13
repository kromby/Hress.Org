using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    public class HardheadSqlAccess : IHardheadDataAccess
    {
        private readonly ILogger<HardheadSqlAccess> _log;
        private readonly string _connectionString;

        public HardheadSqlAccess(DbConnectionInfo connectionInfo, ILogger<HardheadSqlAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<IList<HardheadUser>> GetHardheadUsers(int yearID)
        {
            var sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, uimg.ImageId, COUNT(usr.Id) attended
                        FROM rep_Event hh
                        JOIN    rep_User hhUsr ON hh.Id = hhUsr.EventId
                        JOIN adm_User usr ON hhUsr.UserId = usr.Id
                        LEFT JOIN   upf_Image uimg ON usr.Id = uimg.UserId AND uimg.TypeId = 14
                        WHERE hh.TypeId = 49
                            AND hh.ParentId = @yearID
                        GROUP BY usr.Id, usr.Username, usr.Inserted, uimg.ImageId
                        ORDER BY usr.Username";

            _log.LogInformation("[{Class}] userID: {yearID}", nameof(GetHardheadUsers), yearID);
            _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetHardheadUsers), sql);

            var userList = new List<HardheadUser>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("yearID", yearID);

                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var user = new HardheadUser()
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Username")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted")),
                        Attended = reader.GetInt32(reader.GetOrdinal("attended"))
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                    {
                        user.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("ImageId"));
                    }

                    userList.Add(user);
                }
            }

            return userList;
        }
    }
}
