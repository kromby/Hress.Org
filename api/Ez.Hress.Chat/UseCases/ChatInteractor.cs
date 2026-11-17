using Ez.Hress.Chat.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Chat.UseCases;

public class ChatInteractor
{
    private readonly IChatDataAccess _dataAccess;
    private readonly ILogger<ChatInteractor> _log;

    public ChatInteractor(IChatDataAccess dataAccess, ILogger<ChatInteractor> log)
    {
        _dataAccess = dataAccess;
        _log = log;
    }

    // Returns the thread's messages oldest first, as plain text.
    public async Task<IList<ChatMessage>> GetMessagesAsync(int threadID)
    {
        _log.LogInformation("[{Class}.{Method}] Getting messages for thread {ThreadID}", nameof(ChatInteractor), nameof(GetMessagesAsync), threadID);

        if (threadID <= 0)
            throw new ArgumentOutOfRangeException(nameof(threadID), threadID, "Thread ID must be a positive number.");

        var messages = await _dataAccess.GetMessagesAsync(threadID);
        foreach (var message in messages)
        {
            message.Text = ChatText.ToPlainText(message.Text);
        }

        return messages;
    }

    // The number of messages in each thread, by thread ID. Threads with no messages are left out.
    public async Task<IDictionary<int, int>> GetMessageCountsAsync(IEnumerable<int> threadIDs)
    {
        var ids = threadIDs.Where(id => id > 0).Distinct().ToList();
        _log.LogInformation("[{Class}.{Method}] Counting messages in {Count} threads", nameof(ChatInteractor), nameof(GetMessageCountsAsync), ids.Count);

        if (ids.Count == 0)
            return new Dictionary<int, int>();

        return await _dataAccess.GetMessageCountsAsync(ids);
    }

    public async Task<int> CreateThreadAsync(int categoryID, string name, int userID)
    {
        _log.LogInformation("[{Class}.{Method}] User {UserID} creates thread '{Name}' in category {CategoryID}", nameof(ChatInteractor), nameof(CreateThreadAsync), userID, name, categoryID);

        if (categoryID <= 0)
            throw new ArgumentOutOfRangeException(nameof(categoryID), categoryID, "Category ID must be a positive number.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Thread name is required.", nameof(name));

        return await _dataAccess.CreateThreadAsync(categoryID, name.Trim(), userID, DateTime.UtcNow);
    }

    public async Task DeleteThreadAsync(int threadID, int userID)
    {
        _log.LogInformation("[{Class}.{Method}] User {UserID} deletes thread {ThreadID}", nameof(ChatInteractor), nameof(DeleteThreadAsync), userID, threadID);

        await _dataAccess.DeleteThreadAsync(threadID, userID, DateTime.UtcNow);
    }

    // Posts a plain-text message and returns it as plain text.
    public async Task<ChatMessage> PostMessageAsync(int threadID, int userID, string? text)
    {
        _log.LogInformation("[{Class}.{Method}] User {UserID} posts to thread {ThreadID}", nameof(ChatInteractor), nameof(PostMessageAsync), userID, threadID);

        if (threadID <= 0)
            throw new ArgumentOutOfRangeException(nameof(threadID), threadID, "Thread ID must be a positive number.");
        var plainText = ChatText.Validate(text);

        var messageID = await _dataAccess.AddMessageAsync(threadID, userID, ChatText.ToStoredHtml(plainText), DateTime.UtcNow);
        var message = await _dataAccess.GetMessageAsync(messageID)
            ?? throw new InvalidOperationException($"Message {messageID} was not found after it was saved.");

        message.Text = ChatText.ToPlainText(message.Text);
        return message;
    }
}
