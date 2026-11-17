namespace Ez.Hress.BeerRater.Entities;

// Thrown when a beer with the same normalized name and producer is already in the catalog.
public class DuplicateBeerException(Beer existing) : Exception($"Beer '{existing.Name}' by '{existing.Producer}' already exists.")
{
    public Beer Existing { get; } = existing;
}
