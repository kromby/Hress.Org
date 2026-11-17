using Azure.Data.Tables;
using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.DataAccess;

public class BeerRatingTableAccess : IBeerRatingDataAccess
{
    private readonly TableClient _ratingsTable;
    private readonly ILogger<BeerRatingTableAccess> _logger;
    private readonly string _className;

    public BeerRatingTableAccess(BlobConnectionInfo connectionInfo, ILogger<BeerRatingTableAccess> logger)
    {
        _ratingsTable = new TableClient(connectionInfo.ConnectionString, "BeerRatings");
        _ratingsTable.CreateIfNotExists();
        _logger = logger;
        _className = nameof(BeerRatingTableAccess);
    }

    public async Task<IList<Rating>> GetRatingsAsync(string seasonID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get ratings for season '{SeasonID}'", _className, nameof(GetRatingsAsync), seasonID);

        var encodedSeasonID = Uri.EscapeDataString(seasonID);
        var list = new List<Rating>();
        await foreach (var entity in _ratingsTable.QueryAsync<RatingTableEntity>(e => e.PartitionKey == encodedSeasonID))
        {
            list.Add(entity.ToRating());
        }

        return list;
    }

    public async Task<Rating?> GetRatingAsync(string seasonID, string beerID, int userID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get rating for beer '{BeerID}' by user {UserID} in season '{SeasonID}'", _className, nameof(GetRatingAsync), beerID, userID, seasonID);

        var response = await _ratingsTable.GetEntityIfExistsAsync<RatingTableEntity>(Uri.EscapeDataString(seasonID), RatingTableEntity.CreateRowKey(beerID, userID));
        return response.HasValue ? response.Value!.ToRating() : null;
    }

    public async Task UpsertRatingAsync(Rating rating)
    {
        _logger.LogInformation("[{Class}.{Method}] Save rating for beer '{BeerID}' by user {UserID} in season '{SeasonID}'", _className, nameof(UpsertRatingAsync), rating.BeerID, rating.UserID, rating.SeasonID);

        await _ratingsTable.UpsertEntityAsync(new RatingTableEntity(rating), TableUpdateMode.Replace);
    }
}
