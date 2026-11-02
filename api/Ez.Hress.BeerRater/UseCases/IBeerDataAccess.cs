using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.UseCases;

public interface IBeerDataAccess
{
    Task<IList<Beer>> GetBeersAsync();
}
