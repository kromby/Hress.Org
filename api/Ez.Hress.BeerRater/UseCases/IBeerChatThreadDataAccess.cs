namespace Ez.Hress.BeerRater.UseCases;

// Maps a beer in a season to its Hress chat thread.
public interface IBeerChatThreadDataAccess
{
    // The beer's chat thread, or null if nobody has posted about it yet.
    Task<int?> GetThreadIDAsync(string seasonID, string beerID);
}
