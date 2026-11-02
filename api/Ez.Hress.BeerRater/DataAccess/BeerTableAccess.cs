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
        await foreach (var entity in _beersTable.QueryAsync<BeerTableEntity>(e => e.PartitionKey == BeerTableEntity.Partition))
        {
            list.Add(entity.ToBeer());
        }

        return list;
    }
}
