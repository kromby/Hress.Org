using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

public interface IBeerRatingDataAccess
{
    Task<IList<Rating>> GetRatingsAsync(string seasonID);

    Task<Rating?> GetRatingAsync(string seasonID, string beerID, int userID);

    Task UpsertRatingAsync(Rating rating);
}
