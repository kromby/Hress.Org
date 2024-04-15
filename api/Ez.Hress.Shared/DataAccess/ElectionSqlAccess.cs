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
    public class ElectionSqlAccess : IElectionVoterDataAccess
    {
        private readonly ILogger<ElectionSqlAccess> _log;
        private readonly string _connectionString;

        public ElectionSqlAccess(DbConnectionInfo connectionInfo, ILogger<ElectionSqlAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<VoterEntity?> GetVoter(int userID)
        {
            var sql = @"SELECT	hressuser.Id, hressuser.Username, lastYearVoted.NumericValue 'lastYearVoted', lastStep.NumericValue 'lastStep',
	                        CASE WHEN lastYearVoted.Inserted > lastStep.Inserted THEN lastYearVoted.Inserted ELSE lastStep.Inserted END 'Inserted',
	                        CASE WHEN lastYearVoted.Updated > lastStep.Updated THEN lastYearVoted.Updated ELSE lastStep.Updated END 'Updated'
                        FROM    adm_User hressuser
                        LEFT OUTER JOIN upf_Numeric lastYearVoted ON lastYearVoted.UserId = hressuser.Id AND lastYearVoted.TypeId = 182
                        LEFT OUTER JOIN upf_Numeric lastStep ON lastStep.UserId = hressuser.Id AND lastStep.TypeId = 214
                        WHERE   hressuser.Deleted IS NULL
                        AND hressUser.Id = @userid";

            _log.LogInformation("[{Method}] userID: {userID}", nameof(GetVoter), userID);
            _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(GetVoter), sql);

            using var connection = new SqlConnection(_connectionString);
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

            return null;
        }

        public Task<int> SaveVoter(VoterEntity voter)
        {
            Task<int>? stepThread = null;
            if (voter.LastStepID.HasValue)
                stepThread = SaveNumeric(voter.LastStepID.Value, voter.ID, 214);

            Task<int>? electionThread = null;
            if (voter.LastElectionID.HasValue)
                electionThread = SaveNumeric(voter.LastElectionID.Value, voter.ID, 182);

            try
            {
                stepThread?.Wait();
                electionThread?.Wait();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unable to save voter.");
                throw;
            }

            var result = new Task<int>(() =>
            {
                if ((stepThread == null || stepThread.Result > 0) || (electionThread == null || electionThread.Result > 0))
                    return voter.ID;

                return 0;
            });

            result.Start();

            return result;
        }

        private async Task<int> SaveNumeric(int numericValue, int userId, int typeId)
        {
            var sql = @"UPDATE upf_Numeric SET NumericValue = @numValue, Updated = GetDate(), UpdatedBy = @userId WHERE TypeId = @typeId AND UserId = @userId";
            _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(SaveNumeric), sql);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("numValue", numericValue));
            command.Parameters.Add(new SqlParameter("typeId", typeId));
            command.Parameters.Add(new SqlParameter("userId", userId));

            return await command.ExecuteNonQueryAsync();
        }
    }
}
