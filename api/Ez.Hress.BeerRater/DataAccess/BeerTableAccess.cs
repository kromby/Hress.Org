using Azure.Data.Tables;
using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.DataAccess;

public class BeerTableAccess : IBeerDataAccess
{
    private readonly TableClient _beersTable;
    private readonly ILogger<BeerTableAccess> _logger;
    private readonly string _className;

    public BeerTableAccess(BlobConnectionInfo connectionInfo, ILogger<BeerTableAccess> logger)
    {
        _beersTable = new TableClient(connectionInfo.ConnectionString, "Beers");
        _beersTable.CreateIfNotExists();
        _logger = logger;
        _className = nameof(BeerTableAccess);
    }

    public async Task<IList<Beer>> GetBeersAsync()
    {
        _logger.LogInformation("[{Class}.{Method}] Get beers", _className, nameof(GetBeersAsync));

        var list = new List<Beer>();
        await foreach (var entity in _beersTable.QueryAsync<TableEntity>(e => e.PartitionKey == BeerTableEntity.Partition))
        {
            try
            {
                list.Add(BeerTableEntity.ToBeer(entity));
            }
            catch (FormatException ex)
            {
                // One bad hand-entered row shouldn't break the whole list.
                _logger.LogWarning("[{Class}.{Method}] Skipping beer '{RowKey}': {Message}", _className, nameof(GetBeersAsync), entity.RowKey, ex.Message);
            }
        }

        return list;
    }
}
