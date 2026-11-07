using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Chat.UseCases;
using Microsoft.Extensions.Logging;
using HressChat = Ez.Hress.Chat.Entities;

namespace Ez.Hress.BeerRater.DataAccess;

// The only BeerRater class that uses the Hress chat module. Extracting BeerRater
// means replacing this with calls to the Hress chat API.
public class HressChatAccess : IBeerChatDataAccess
{
    public const string CategorySetting = "Ez.Hress.BeerRater.ChatCategoryID";

    private readonly ChatInteractor _chatInteractor;
    private readonly int _categoryID;
    private readonly ILogger<HressChatAccess> _logger;
    private readonly string _className;

    // categoryID is the "Beer Rater" chat category (adm_Component), from the CategorySetting app setting.
    public HressChatAccess(ChatInteractor chatInteractor, int categoryID, ILogger<HressChatAccess> logger)
    {
        _chatInteractor = chatInteractor;
        _categoryID = categoryID;
        _logger = logger;
        _className = nameof(HressChatAccess);
    }

    public async Task<IList<ChatMessage>> GetMessagesAsync(int threadID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get messages for thread {ThreadID}", _className, nameof(GetMessagesAsync), threadID);

        var messages = await _chatInteractor.GetMessagesAsync(threadID);
        return messages.Select(ToChatMessage).ToList();
    }

    public async Task<IDictionary<int, int>> GetMessageCountsAsync(IReadOnlyCollection<int> threadIDs)
    {
        _logger.LogInformation("[{Class}.{Method}] Count messages in {Count} threads", _className, nameof(GetMessageCountsAsync), threadIDs.Count);

        return await _chatInteractor.GetMessageCountsAsync(threadIDs);
    }

    public async Task<int> CreateThreadAsync(string name, int userID)
    {
        _logger.LogInformation("[{Class}.{Method}] Create thread '{Name}'", _className, nameof(CreateThreadAsync), name);

        // A missing category is a configuration error, not a bad request or an unknown beer.
        if (_categoryID <= 0)
            throw new InvalidOperationException($"App setting '{CategorySetting}' is not set.");

        try
        {
            return await _chatInteractor.CreateThreadAsync(_categoryID, name, userID);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException($"App setting '{CategorySetting}' doesn't point to a chat category.", ex);
        }
    }

    public async Task DeleteThreadAsync(int threadID, int userID)
    {
        _logger.LogInformation("[{Class}.{Method}] Delete thread {ThreadID}", _className, nameof(DeleteThreadAsync), threadID);

        await _chatInteractor.DeleteThreadAsync(threadID, userID);
    }

    public async Task<ChatMessage> PostMessageAsync(int threadID, int userID, string text)
    {
        _logger.LogInformation("[{Class}.{Method}] Post message to thread {ThreadID}", _className, nameof(PostMessageAsync), threadID);

        var message = await _chatInteractor.PostMessageAsync(threadID, userID, text);
        return ToChatMessage(message);
    }

    private static ChatMessage ToChatMessage(HressChat.ChatMessage message) => new()
    {
        ID = message.ID,
        UserID = message.UserID,
        Username = message.Username,
        AuthorImageID = message.AuthorImageID,
        Text = message.Text,
        Created = message.Inserted,
        Edited = message.Updated,
    };
}
