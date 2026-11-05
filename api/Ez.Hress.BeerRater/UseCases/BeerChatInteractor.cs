using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class BeerChatInteractor
{
    public const int MaxTextLength = 2000;

    private readonly IBeerChatThreadDataAccess _threadDataAccess;
    private readonly IBeerChatDataAccess _chatDataAccess;
    private readonly BeerInteractor _beerInteractor;
    private readonly SeasonInteractor _seasonInteractor;
    private readonly ILogger<BeerChatInteractor> _log;

    public BeerChatInteractor(IBeerChatThreadDataAccess threadDataAccess, IBeerChatDataAccess chatDataAccess, BeerInteractor beerInteractor, SeasonInteractor seasonInteractor, ILogger<BeerChatInteractor> log)
    {
        _threadDataAccess = threadDataAccess;
        _chatDataAccess = chatDataAccess;
        _beerInteractor = beerInteractor;
        _seasonInteractor = seasonInteractor;
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

    public async Task<ChatMessage> PostMessageAsync(string seasonID, string beerID, int userID, string? text)
    {
        _log.LogInformation("[{Class}.{Method}] User {UserID} posts about beer '{BeerID}' in season '{SeasonID}'", nameof(BeerChatInteractor), nameof(PostMessageAsync), userID, beerID, seasonID);

        // Checked before a thread is created, so an invalid first message doesn't leave an empty thread.
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new ArgumentException("Message text is required.", nameof(text));
        if (trimmed.Length > MaxTextLength)
            throw new ArgumentException($"Message text can be at most {MaxTextLength} characters.", nameof(text));

        // Throws KeyNotFoundException for an unknown season and SeasonClosedException for a closed one.
        var season = await _seasonInteractor.GetOpenSeasonAsync(seasonID);
        // Throws KeyNotFoundException for a beer that isn't in the season.
        var beer = await _beerInteractor.GetSeasonBeerAsync(seasonID, beerID);

        var threadID = await GetOrCreateThreadAsync(season, beer, userID);
        return await _chatDataAccess.PostMessageAsync(threadID, userID, trimmed);
    }

    private async Task<int> GetOrCreateThreadAsync(Season season, Beer beer, int userID)
    {
        var threadID = await _threadDataAccess.GetThreadIDAsync(season.ID, beer.ID);
        if (threadID != null)
            return threadID.Value;

        var seasonName = string.IsNullOrWhiteSpace(season.Name) ? season.ID : season.Name;
        var newThreadID = await _chatDataAccess.CreateThreadAsync($"{beer.Name} ({seasonName})", userID);
        if (await _threadDataAccess.TryAddThreadAsync(season.ID, beer.ID, newThreadID))
            return newThreadID;

        // Another member's first message got there first: drop the empty thread and use theirs.
        _log.LogInformation("[{Class}.{Method}] Beer '{BeerID}' got a thread at the same time, removing thread {ThreadID}", nameof(BeerChatInteractor), nameof(GetOrCreateThreadAsync), beer.ID, newThreadID);
        await _chatDataAccess.DeleteThreadAsync(newThreadID, userID);

        return await _threadDataAccess.GetThreadIDAsync(season.ID, beer.ID)
            ?? throw new InvalidOperationException($"Chat thread for beer '{beer.ID}' in season '{season.ID}' not found.");
    }
}
