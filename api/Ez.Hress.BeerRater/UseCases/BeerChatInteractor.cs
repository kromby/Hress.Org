using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class BeerChatInteractor
{
    private readonly IBeerChatThreadDataAccess _threadDataAccess;
    private readonly IBeerChatDataAccess _chatDataAccess;
    private readonly BeerInteractor _beerInteractor;
    private readonly ILogger<BeerChatInteractor> _log;

    public BeerChatInteractor(IBeerChatThreadDataAccess threadDataAccess, IBeerChatDataAccess chatDataAccess, BeerInteractor beerInteractor, ILogger<BeerChatInteractor> log)
    {
        _threadDataAccess = threadDataAccess;
        _chatDataAccess = chatDataAccess;
        _beerInteractor = beerInteractor;
        _log = log;
    }

    // Everyone sees the same chat, rated or not, also after the season has ended.
    public async Task<IList<ChatMessage>> GetMessagesAsync(string seasonID, string beerID)
    {
        _log.LogInformation("[{Class}.{Method}] Getting chat for beer '{BeerID}' in season '{SeasonID}'", nameof(BeerChatInteractor), nameof(GetMessagesAsync), beerID, seasonID);

        // Throws KeyNotFoundException for an unknown season or a beer that isn't in the season.
        await _beerInteractor.GetSeasonBeerAsync(seasonID, beerID);

        // The thread is created with the first message.
        var threadID = await _threadDataAccess.GetThreadIDAsync(seasonID, beerID);
        if (threadID == null)
            return new List<ChatMessage>();

        return await _chatDataAccess.GetMessagesAsync(threadID.Value);
    }
}
