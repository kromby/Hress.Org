namespace Ez.Hress.BeerRater.Entities;

public class SeasonClosedException : InvalidOperationException
{
    public SeasonClosedException(string seasonID)
        : base($"Season '{seasonID}' is closed.")
    {
    }
}
