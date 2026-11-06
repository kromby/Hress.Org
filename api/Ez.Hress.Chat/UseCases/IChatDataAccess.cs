using Ez.Hress.Chat.Entities;

namespace Ez.Hress.Chat.UseCases;

public interface IChatDataAccess
{
    // The thread's messages that aren't deleted, oldest first, with Text as stored (HTML).
    Task<IList<ChatMessage>> GetMessagesAsync(int threadID);

    // A message that isn't deleted, with Text as stored (HTML), or null.
    Task<ChatMessage?> GetMessageAsync(int messageID);

    // Creates a thread in a top-level chat category and returns its ID.
    // Throws KeyNotFoundException if the category doesn't exist.
    Task<int> CreateThreadAsync(int categoryID, string name, int userID, DateTime created);

    // Soft-deletes a thread.
    Task DeleteThreadAsync(int threadID, int userID, DateTime deleted);

    // Adds a message (Text as HTML) to a thread and marks the thread updated. Returns the message ID.
    // Throws KeyNotFoundException if the thread doesn't exist.
    Task<int> AddMessageAsync(int threadID, int userID, string html, DateTime created);
}
