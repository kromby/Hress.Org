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
}
