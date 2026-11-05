using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

// Chat messages live in the shared Hress chat, not in BeerRater's own storage.
public interface IBeerChatDataAccess
{
    // The thread's messages, oldest first, without deleted ones.
    Task<IList<ChatMessage>> GetMessagesAsync(int threadID);
}
