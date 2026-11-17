using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

public interface IBeerDataAccess
{
    Task<IList<Beer>> GetBeersAsync();

    // Throws InvalidOperationException if a beer with that ID already exists.
    Task AddBeerAsync(Beer beer);
}
