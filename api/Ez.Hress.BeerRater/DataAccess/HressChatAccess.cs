using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Chat.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.DataAccess;

// The only BeerRater class that uses the Hress chat module. Extracting BeerRater
// means replacing this with calls to the Hress chat API.
public class HressChatAccess : IBeerChatDataAccess
{
    private readonly ChatInteractor _chatInteractor;
    private readonly ILogger<HressChatAccess> _logger;
    private readonly string _className;

    public HressChatAccess(ChatInteractor chatInteractor, ILogger<HressChatAccess> logger)
    {
        _chatInteractor = chatInteractor;
        _logger = logger;
        _className = nameof(HressChatAccess);
    }

    public async Task<IList<ChatMessage>> GetMessagesAsync(int threadID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get messages for thread {ThreadID}", _className, nameof(GetMessagesAsync), threadID);

        var messages = await _chatInteractor.GetMessagesAsync(threadID);
        return messages.Select(m => new ChatMessage
        {
            ID = m.ID,
            UserID = m.UserID,
            Username = m.Username,
            AuthorImageID = m.AuthorImageID,
            Text = m.Text,
            Created = m.Inserted,
            Edited = m.Updated,
        }).ToList();
    }
}
