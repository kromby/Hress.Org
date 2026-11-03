using Azure.Data.Tables;
using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.DataAccess;

public class BeerSeasonTableAccess : IBeerSeasonDataAccess
{
    private readonly TableClient _seasonsTable;
    private readonly TableClient _seasonBeersTable;
    private readonly ILogger<BeerSeasonTableAccess> _logger;
    private readonly string _className;

    public BeerSeasonTableAccess(BlobConnectionInfo connectionInfo, ILogger<BeerSeasonTableAccess> logger)
    {
        _seasonsTable = new TableClient(connectionInfo.ConnectionString, "BeerSeasons");
        _seasonsTable.CreateIfNotExists();
        _seasonBeersTable = new TableClient(connectionInfo.ConnectionString, "BeerSeasonBeers");
        _seasonBeersTable.CreateIfNotExists();
        _logger = logger;
        _className = nameof(BeerSeasonTableAccess);
    }

    public async Task<IList<Season>> GetSeasonsAsync()
    {
        _logger.LogInformation("[{Class}.{Method}] Get seasons", _className, nameof(GetSeasonsAsync));

        var list = new List<Season>();
        await foreach (var entity in _seasonsTable.QueryAsync<TableEntity>(e => e.PartitionKey == SeasonTableEntity.Partition))
        {
            try
            {
                list.Add(SeasonTableEntity.ToSeason(entity));
            }
            catch (FormatException ex)
            {
                // One bad hand-entered row shouldn't break the season lookup.
                _logger.LogWarning("[{Class}.{Method}] Skipping season '{RowKey}': {Message}", _className, nameof(GetSeasonsAsync), entity.RowKey, ex.Message);
            }
        }

        return list;
    }

    public async Task<IList<string>> GetSeasonBeerIDsAsync(string seasonID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get beer IDs for season '{SeasonID}'", _className, nameof(GetSeasonBeerIDsAsync), seasonID);

        var encodedSeasonID = Uri.EscapeDataString(seasonID);
        var list = new List<string>();
        await foreach (var entity in _seasonBeersTable.QueryAsync<SeasonBeerTableEntity>(e => e.PartitionKey == encodedSeasonID))
        {
            list.Add(Uri.UnescapeDataString(entity.RowKey));
        }

        return list;
    }

    public async Task AddBeerToSeasonAsync(string seasonID, string beerID)
    {
        _logger.LogInformation("[{Class}.{Method}] Add beer '{BeerID}' to season '{SeasonID}'", _className, nameof(AddBeerToSeasonAsync), beerID, seasonID);

        await _seasonBeersTable.UpsertEntityAsync(new SeasonBeerTableEntity(seasonID, beerID));
    }
}
