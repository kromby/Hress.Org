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
}
