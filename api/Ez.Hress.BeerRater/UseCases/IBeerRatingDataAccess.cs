using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

public interface IBeerRatingDataAccess
{
    Task<IList<Rating>> GetRatingsAsync(string seasonID);
}
