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

    private const string SelectMessages = @"SELECT	msg.Id, msg.ComponentId, msg.InsertedBy, msg.Inserted, msg.Updated, msg.TextValue, author.Username, userPhoto.ImageId 'AuthorImageID'
                        FROM	scr_Text msg
                        JOIN	adm_Component thread ON thread.Id = msg.ComponentId AND thread.TypeId = @chatTypeId AND thread.Deleted IS NULL
                        JOIN	adm_User author ON author.Id = msg.InsertedBy
                        LEFT OUTER JOIN	upf_Image userPhoto ON author.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        WHERE	msg.TypeId = @bodyTypeId
                            AND	msg.Deleted IS NULL";

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
        const string sql = SelectMessages + @"
                            AND	msg.ComponentId = @threadId
                        ORDER BY msg.Inserted, msg.Id";

        _log.LogInformation("[{Class}.{Method}] Get messages for thread {ThreadID}", _className, nameof(GetMessagesAsync), threadID);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = CreateMessageCommand(sql, connection);
        command.Parameters.AddWithValue("threadId", threadID);

        var list = new List<ChatMessage>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ParseMessage(reader));
        }

        return list;
    }

    public async Task<ChatMessage?> GetMessageAsync(int messageID)
    {
        const string sql = SelectMessages + @"
                            AND	msg.Id = @messageId";

        _log.LogInformation("[{Class}.{Method}] Get message {MessageID}", _className, nameof(GetMessageAsync), messageID);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = CreateMessageCommand(sql, connection);
        command.Parameters.AddWithValue("messageId", messageID);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ParseMessage(reader) : null;
    }

    public async Task<int> CreateThreadAsync(int categoryID, string name, int userID, DateTime created)
    {
        // Inserts nothing unless the category is a top-level chat that isn't deleted.
        const string sql = @"INSERT INTO adm_Component (TypeId, GroupType, IsPublic, Name, Hits, ParentId, InsertedBy, Inserted)
                        OUTPUT INSERTED.Id
                        SELECT	@chatTypeId, 'CONTENT', 0, @name, 0, category.Id, @userId, @created
                        FROM	adm_Component category
                        WHERE	category.Id = @categoryId
                            AND	category.TypeId = @chatTypeId
                            AND	category.ParentId IS NULL
                            AND	category.Deleted IS NULL";

        _log.LogInformation("[{Class}.{Method}] Create thread '{Name}' in category {CategoryID}", _className, nameof(CreateThreadAsync), name, categoryID);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("chatTypeId", ChatTypeID);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("categoryId", categoryID);
        command.Parameters.AddWithValue("userId", userID);
        command.Parameters.AddWithValue("created", created);

        var id = await command.ExecuteScalarAsync();
        if (id == null || id == DBNull.Value)
            throw new KeyNotFoundException($"Chat category {categoryID} not found.");

        return Convert.ToInt32(id);
    }

    public async Task DeleteThreadAsync(int threadID, int userID, DateTime deleted)
    {
        const string sql = @"UPDATE	adm_Component
                        SET	Deleted = @deleted, DeletedBy = @userId
                        WHERE	Id = @threadId
                            AND	TypeId = @chatTypeId
                            AND	Deleted IS NULL";

        _log.LogInformation("[{Class}.{Method}] Delete thread {ThreadID}", _className, nameof(DeleteThreadAsync), threadID);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("deleted", deleted);
        command.Parameters.AddWithValue("userId", userID);
        command.Parameters.AddWithValue("threadId", threadID);
        command.Parameters.AddWithValue("chatTypeId", ChatTypeID);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> AddMessageAsync(int threadID, int userID, string html, DateTime created)
    {
        // Inserts nothing unless the thread is a chat thread that isn't deleted.
        const string insertSql = @"INSERT INTO scr_Text (ComponentId, TypeId, TextValue, InsertedBy, Inserted)
                        OUTPUT INSERTED.Id
                        SELECT	thread.Id, @bodyTypeId, @textValue, @userId, @created
                        FROM	adm_Component thread
                        WHERE	thread.Id = @threadId
                            AND	thread.TypeId = @chatTypeId
                            AND	thread.Deleted IS NULL";
        // The legacy chat sorts threads by when they were last updated.
        const string touchThreadSql = @"UPDATE	adm_Component
                        SET	Updated = @created, UpdatedBy = @userId
                        WHERE	Id = @threadId";

        _log.LogInformation("[{Class}.{Method}] Add message to thread {ThreadID}", _className, nameof(AddMessageAsync), threadID);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            using var insertCommand = new SqlCommand(insertSql, connection, transaction);
            insertCommand.Parameters.AddWithValue("bodyTypeId", BodyTextTypeID);
            insertCommand.Parameters.AddWithValue("textValue", html);
            insertCommand.Parameters.AddWithValue("userId", userID);
            insertCommand.Parameters.AddWithValue("created", created);
            insertCommand.Parameters.AddWithValue("threadId", threadID);
            insertCommand.Parameters.AddWithValue("chatTypeId", ChatTypeID);

            var id = await insertCommand.ExecuteScalarAsync();
            if (id == null || id == DBNull.Value)
                throw new KeyNotFoundException($"Chat thread {threadID} not found.");
            var messageID = Convert.ToInt32(id);

            using var touchThreadCommand = new SqlCommand(touchThreadSql, connection, transaction);
            touchThreadCommand.Parameters.AddWithValue("created", created);
            touchThreadCommand.Parameters.AddWithValue("userId", userID);
            touchThreadCommand.Parameters.AddWithValue("threadId", threadID);
            await touchThreadCommand.ExecuteNonQueryAsync();

            transaction.Commit();
            return messageID;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static SqlCommand CreateMessageCommand(string sql, SqlConnection connection)
    {
        var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("chatTypeId", ChatTypeID);
        command.Parameters.AddWithValue("bodyTypeId", BodyTextTypeID);
        return command;
    }

    private static ChatMessage ParseMessage(SqlDataReader reader) => new()
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
    };
}
