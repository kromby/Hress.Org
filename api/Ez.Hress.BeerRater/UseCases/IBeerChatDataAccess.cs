using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

// Chat messages live in the shared Hress chat, not in BeerRater's own storage.
public interface IBeerChatDataAccess
{
    // The thread's messages, oldest first, without deleted ones.
    Task<IList<ChatMessage>> GetMessagesAsync(int threadID);

    // Creates a thread in BeerRater's chat category and returns its ID.
    Task<int> CreateThreadAsync(string name, int userID);

    // Removes a thread nobody has posted to.
    Task DeleteThreadAsync(int threadID, int userID);

    // Posts a plain-text message and returns it.
    Task<ChatMessage> PostMessageAsync(int threadID, int userID, string text);
}
