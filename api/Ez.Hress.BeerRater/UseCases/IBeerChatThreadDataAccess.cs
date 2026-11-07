namespace Ez.Hress.BeerRater.UseCases;

// Maps a beer in a season to its Hress chat thread.
public interface IBeerChatThreadDataAccess
{
    // The beer's chat thread, or null if nobody has posted about it yet.
    Task<int?> GetThreadIDAsync(string seasonID, string beerID);

    // The season's chat threads by beer ID. Beers nobody has posted about are left out.
    Task<IDictionary<string, int>> GetThreadIDsAsync(string seasonID);

    // Saves the beer's chat thread. Returns false, without saving, if the beer already has one.
    Task<bool> TryAddThreadAsync(string seasonID, string beerID, int threadID);
}
