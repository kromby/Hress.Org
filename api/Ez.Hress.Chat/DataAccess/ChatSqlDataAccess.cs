using Ez.Hress.Chat.Entities;
using Ez.Hress.Chat.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Chat.DataAccess;

// Chat threads are adm_Component rows (TypeId 10) whose ParentId is their category,
// and messages are scr_Text rows (TypeId 36, TXT_BODY) on the thread, as in the legacy chat.
public class ChatSqlDataAccess : IChatDataAccess
{
    private const int ChatTypeID = 10;
    private const int BodyTextTypeID = 36;

    private readonly string _connectionString;
    private readonly ILogger<ChatSqlDataAccess> _log;
    private readonly string _className;

    public ChatSqlDataAccess(DbConnectionInfo dbConnection, ILogger<ChatSqlDataAccess> log)
    {
        _connectionString = dbConnection.ConnectionString;
        _log = log;
        _className = nameof(ChatSqlDataAccess);
    }

    public async Task<IList<ChatMessage>> GetMessagesAsync(int threadID)
    {
        const string sql = @"SELECT	msg.Id, msg.ComponentId, msg.InsertedBy, msg.Inserted, msg.Updated, msg.TextValue, author.Username, userPhoto.ImageId 'AuthorImageID'
                        FROM	scr_Text msg
                        JOIN	adm_Component thread ON thread.Id = msg.ComponentId AND thread.TypeId = @chatTypeId AND thread.Deleted IS NULL
                        JOIN	adm_User author ON author.Id = msg.InsertedBy
                        LEFT OUTER JOIN	upf_Image userPhoto ON author.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        WHERE	msg.ComponentId = @threadId
                            AND	msg.TypeId = @bodyTypeId
                            AND	msg.Deleted IS NULL
                        ORDER BY msg.Inserted, msg.Id";

        _log.LogInformation("[{Class}.{Method}] Get messages for thread {ThreadID}", _className, nameof(GetMessagesAsync), threadID);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("threadId", threadID);
        command.Parameters.AddWithValue("chatTypeId", ChatTypeID);
        command.Parameters.AddWithValue("bodyTypeId", BodyTextTypeID);

        var list = new List<ChatMessage>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ChatMessage
            {
                ID = Convert.ToInt32(reader["Id"]),
                ThreadID = Convert.ToInt32(reader["ComponentId"]),
                UserID = Convert.ToInt32(reader["InsertedBy"]),
                Username = reader["Username"]?.ToString() ?? string.Empty,
                AuthorImageID = reader["AuthorImageID"] == DBNull.Value ? null : Convert.ToInt32(reader["AuthorImageID"]),
                Text = reader["TextValue"]?.ToString() ?? string.Empty,
                // Stored as Icelandic local time, which is UTC all year.
                Inserted = DateTime.SpecifyKind(Convert.ToDateTime(reader["Inserted"]), DateTimeKind.Utc),
                Updated = reader["Updated"] == DBNull.Value ? null : DateTime.SpecifyKind(Convert.ToDateTime(reader["Updated"]), DateTimeKind.Utc),
            });
        }

        return list;
    }
}
