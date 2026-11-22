using Ez.Hress.BeerRater.Entities;
using Ez.Hress.Notifications.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class BeerChatInteractor
{
    public const int MaxTextLength = 2000;

    private readonly IBeerChatThreadDataAccess _threadDataAccess;
    private readonly IBeerChatDataAccess _chatDataAccess;
    private readonly BeerInteractor _beerInteractor;
    private readonly SeasonInteractor _seasonInteractor;
    private readonly BeerActivityInteractor _activityInteractor;
    private readonly NotificationInteractor _notificationInteractor;
    private readonly ILogger<BeerChatInteractor> _log;
    private readonly TimeSpan _notifyTimeout;

    public BeerChatInteractor(IBeerChatThreadDataAccess threadDataAccess, IBeerChatDataAccess chatDataAccess, BeerInteractor beerInteractor, SeasonInteractor seasonInteractor,
        BeerActivityInteractor activityInteractor, NotificationInteractor notificationInteractor, ILogger<BeerChatInteractor> log, TimeSpan? notifyTimeout = null)
    {
        _threadDataAccess = threadDataAccess;
        _chatDataAccess = chatDataAccess;
        _beerInteractor = beerInteractor;
        _seasonInteractor = seasonInteractor;
        _activityInteractor = activityInteractor;
        _notificationInteractor = notificationInteractor;
        _log = log;
        _notifyTimeout = notifyTimeout ?? TimeSpan.FromSeconds(5);
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

    // The number of messages about each beer in the season, by beer ID. Beers nobody has
    // posted about are left out.
    public async Task<IDictionary<string, int>> GetMessageCountsAsync(string seasonID)
    {
        _log.LogInformation("[{Class}.{Method}] Counting chat messages in season '{SeasonID}'", nameof(BeerChatInteractor), nameof(GetMessageCountsAsync), seasonID);

        var threads = await _threadDataAccess.GetThreadIDsAsync(seasonID);
        if (threads.Count == 0)
            return new Dictionary<string, int>(StringComparer.Ordinal);

        var counts = await _chatDataAccess.GetMessageCountsAsync(threads.Values.Distinct().ToList());
        return threads
            .Where(t => counts.ContainsKey(t.Value))
            .ToDictionary(t => t.Key, t => counts[t.Value], StringComparer.Ordinal);
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
        var message = await _chatDataAccess.PostMessageAsync(threadID, userID, trimmed);

        // Best effort: the message is already posted, so a failing or slow notification is only logged.
        try
        {
            await NotifyCommentAsync(beer, message, userID, trimmed).WaitAsync(_notifyTimeout);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Failed to notify about message {MessageID} on beer '{BeerID}'", nameof(BeerChatInteractor), nameof(PostMessageAsync), message.ID, beer.ID);
        }

        return message;
    }

    // Tells the members active in Ölrýnir, except the commenter, about the comment.
    private async Task NotifyCommentAsync(Beer beer, ChatMessage message, int userID, string text)
    {
        var since = BeerActivityInteractor.ActiveSince(DateTime.UtcNow);
        var recipients = (await _activityInteractor.GetActiveUserIDsAsync(since)).Where(id => id != userID).ToList();
        if (recipients.Count == 0)
            return;

        var messageText = string.IsNullOrWhiteSpace(message.Text) ? text : message.Text;
        await _notificationInteractor.NotifyBeerCommentAsync(beer.ID, beer.Name, message.Username, messageText, recipients);
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
