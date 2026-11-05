using Ez.Hress.Chat.Entities;

namespace Ez.Hress.Chat.UseCases;

public interface IChatDataAccess
{
    // The thread's messages that aren't deleted, oldest first, with Text as stored (HTML).
    Task<IList<ChatMessage>> GetMessagesAsync(int threadID);
}
