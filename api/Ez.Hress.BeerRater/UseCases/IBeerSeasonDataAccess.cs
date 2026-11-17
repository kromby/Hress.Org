using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

public interface IBeerSeasonDataAccess
{
    Task<IList<Season>> GetSeasonsAsync();

    Task<IList<string>> GetSeasonBeerIDsAsync(string seasonID);

    Task AddBeerToSeasonAsync(string seasonID, string beerID);
}
