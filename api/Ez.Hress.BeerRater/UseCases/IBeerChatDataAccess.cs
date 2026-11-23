using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

// Chat messages live in the shared Hress chat, not in BeerRater's own storage.
public interface IBeerChatDataAccess
{
    // The thread's messages, oldest first, without deleted ones.
    Task<IList<ChatMessage>> GetMessagesAsync(int threadID);

    // The number of messages in each thread, by thread ID. Threads with no messages are left out.
    Task<IDictionary<int, int>> GetMessageCountsAsync(IReadOnlyCollection<int> threadIDs);

    // The distinct IDs of users who have posted in BeerRater's chat category since the given time (UTC).
    Task<IList<int>> GetPosterIDsAsync(DateTime since);

    // Creates a thread in BeerRater's chat category and returns its ID.
    Task<int> CreateThreadAsync(string name, int userID);

    // Removes a thread nobody has posted to.
    Task DeleteThreadAsync(int threadID, int userID);

    // Posts a plain-text message and returns it.
    Task<ChatMessage> PostMessageAsync(int threadID, int userID, string text);
}
