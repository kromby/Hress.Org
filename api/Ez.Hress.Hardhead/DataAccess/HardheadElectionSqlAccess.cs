using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Hardhead.DataAccess;

public class HardheadElectionSqlAccess : IHardheadElectionDataAccess
{
    private readonly ILogger<HardheadElectionSqlAccess> _log;
    private readonly string _connectionString;

    public HardheadElectionSqlAccess(DbConnectionInfo connectionInfo, ILogger<HardheadElectionSqlAccess> log)
    {
        _connectionString = connectionInfo.ConnectionString;
        _log = log;
    }

    public async Task<int> SaveVote(Vote entity)
    {
        var sql = @"INSERT INTO [dbo].[rep_PollVote]
                ([EventId], [TypeId], [Value], [Inserted], [TextValue])
                VALUES (@EventId, @TypeId, @Value, @Inserted, @TextValue)";
        if (entity.PollEntryID.HasValue)
        {
            sql = @"INSERT INTO [dbo].[rep_PollVote]
                ([EventId], [TypeId], [Value], [Inserted], [TextValue], [PollEntryId])
                VALUES (@EventId, @TypeId, @Value, @Inserted, @TextValue, @PollEntryId)";
        }

        _log.LogInformation("[{Method}] userID: {EventID}", nameof(SaveVote), entity.EventID);
        _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(SaveVote), sql);

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("EventId", entity.EventID));
        command.Parameters.Add(new SqlParameter("TypeId", 49));
        command.Parameters.Add(new SqlParameter("Value", entity.Value));
        command.Parameters.Add(new SqlParameter("Inserted", entity.Created));
        if (entity.PollEntryID.HasValue)
            command.Parameters.Add(new SqlParameter("PollEntryId", entity.PollEntryID.Value));
        if (string.IsNullOrWhiteSpace(entity.Description))
            command.Parameters.Add(new SqlParameter("TextValue", DBNull.Value));
        else
            command.Parameters.Add(new SqlParameter("TextValue", entity.Description));

        return await command.ExecuteNonQueryAsync();
    }
}
