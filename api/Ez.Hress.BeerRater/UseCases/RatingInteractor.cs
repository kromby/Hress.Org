using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class RatingInteractor
{
    public const int MinStars = 1;
    public const int MaxStars = 5;

    private readonly IBeerRatingDataAccess _ratingDataAccess;
    private readonly BeerInteractor _beerInteractor;
    private readonly SeasonInteractor _seasonInteractor;
    private readonly ILogger<RatingInteractor> _log;

    public RatingInteractor(IBeerRatingDataAccess ratingDataAccess, BeerInteractor beerInteractor, SeasonInteractor seasonInteractor, ILogger<RatingInteractor> log)
    {
        _ratingDataAccess = ratingDataAccess;
        _beerInteractor = beerInteractor;
        _seasonInteractor = seasonInteractor;
        _log = log;
    }

    public async Task<Rating> SaveRatingAsync(string seasonID, string beerID, int userID, int stars)
    {
        _log.LogInformation("[{Class}.{Method}] User {UserID} rates beer '{BeerID}' in season '{SeasonID}' with {Stars} stars", nameof(RatingInteractor), nameof(SaveRatingAsync), userID, beerID, seasonID, stars);

        if (stars < MinStars || stars > MaxStars)
            throw new ArgumentOutOfRangeException(nameof(stars), stars, $"Stars must be a whole number from {MinStars} to {MaxStars}.");

        // Throws KeyNotFoundException for an unknown season and SeasonClosedException for a closed one.
        await _seasonInteractor.GetOpenSeasonAsync(seasonID);
        // Throws KeyNotFoundException for a beer that isn't in the season.
        await _beerInteractor.GetSeasonBeerAsync(seasonID, beerID);

        // One rating per user, per beer, per season: rating again replaces the earlier one.
        var now = DateTime.UtcNow;
        var existing = await _ratingDataAccess.GetRatingAsync(seasonID, beerID, userID);
        var rating = new Rating
        {
            SeasonID = seasonID,
            BeerID = beerID,
            UserID = userID,
            Stars = stars,
            Created = existing?.Created ?? now,
            Updated = now
        };

        await _ratingDataAccess.UpsertRatingAsync(rating);
        return rating;
    }
}
