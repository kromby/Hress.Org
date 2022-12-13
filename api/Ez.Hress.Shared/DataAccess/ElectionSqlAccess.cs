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
    public class ElectionSqlAccess : IElectionDataAccess
    {
        private readonly ILogger<ElectionSqlAccess> _log;
        private readonly string _connectionString;

        public ElectionSqlAccess(DbConnectionInfo connectionInfo, ILogger<ElectionSqlAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<VoterEntity> GetVoter(int userID)
        {
            var sql = @"SELECT	hressuser.Id, hressuser.Username, lastYearVoted.NumericValue 'lastYearVoted', lastStep.NumericValue 'lastStep',
	                        CASE WHEN lastYearVoted.Inserted > lastStep.Inserted THEN lastYearVoted.Inserted ELSE lastStep.Inserted END 'Inserted',
	                        CASE WHEN lastYearVoted.Updated > lastStep.Updated THEN lastYearVoted.Updated ELSE lastStep.Updated END 'Updated'
                        FROM    adm_User hressuser
                        LEFT OUTER JOIN upf_Numeric lastYearVoted ON lastYearVoted.UserId = hressuser.Id AND lastYearVoted.TypeId = 182
                        LEFT OUTER JOIN upf_Numeric lastStep ON lastStep.UserId = hressuser.Id AND lastStep.TypeId = 214
                        WHERE   hressuser.Deleted IS NULL
                        AND hressUser.Id = @userid";

            _log.LogInformation("[{Class}] userID: {userID}", nameof(GetVoter), userID);
            _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetVoter), sql);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("userid", userID));

                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var entity = new VoterEntity()
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("Id")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        LastElectionID = SqlHelper.GetNullableDecimalToInt(reader, "lastYearVoted"),
                        LastStepID = SqlHelper.GetNullableDecimalToInt(reader, "lastStep"),
                        Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
                        Updated = SqlHelper.GetDateTimeNullable(reader, "Updated")
                    };

                    return entity;
                }
            }

            return null;
        }
    }
}
