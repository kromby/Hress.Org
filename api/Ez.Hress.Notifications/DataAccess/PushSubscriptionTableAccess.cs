using Azure;
using Azure.Data.Tables;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.DataAccess;

public class PushSubscriptionTableAccess : IPushSubscriptionDataAccess
{
    private readonly TableClient _table;
    private readonly EnsureCreatedOnce _ensureCreated;
    private readonly ILogger<PushSubscriptionTableAccess> _logger;
    private readonly string _className;

    public PushSubscriptionTableAccess(BlobConnectionInfo connectionInfo, ILogger<PushSubscriptionTableAccess> logger)
    {
        _table = new TableClient(connectionInfo.ConnectionString, "PushSubscriptions");
        _ensureCreated = new EnsureCreatedOnce(() => _table.CreateIfNotExistsAsync());
        _logger = logger;
        _className = nameof(PushSubscriptionTableAccess);
    }

    public async Task UpsertAsync(PushSubscription subscription)
    {
        _logger.LogInformation("[{Class}.{Method}] Upsert subscription for user {UserID}", _className, nameof(UpsertAsync), subscription.UserID);
        await _ensureCreated.RunAsync();

        // One row per device: same endpoint replaces the row, whichever member saved it.
        await _table.UpsertEntityAsync(new PushSubscriptionTableEntity(subscription), TableUpdateMode.Replace);
    }

    public async Task<PushSubscription?> GetAsync(string endpoint)
    {
        _logger.LogInformation("[{Class}.{Method}] Get subscription", _className, nameof(GetAsync));
        await _ensureCreated.RunAsync();

        var response = await _table.GetEntityIfExistsAsync<PushSubscriptionTableEntity>(
            NotificationKeys.AllPartition, NotificationKeys.EndpointKey(endpoint));
        return response.HasValue ? response.Value!.ToSubscription() : null;
    }

    public async Task DeleteAsync(string endpoint)
    {
        _logger.LogInformation("[{Class}.{Method}] Delete subscription", _className, nameof(DeleteAsync));
        await _ensureCreated.RunAsync();

        await _table.DeleteEntityAsync(NotificationKeys.AllPartition, NotificationKeys.EndpointKey(endpoint));
    }

    public async Task<IList<PushSubscription>> GetAllAsync()
    {
        _logger.LogInformation("[{Class}.{Method}] Get all subscriptions", _className, nameof(GetAllAsync));
        await _ensureCreated.RunAsync();

        var list = new List<PushSubscription>();
        await foreach (var entity in _table.QueryAsync<PushSubscriptionTableEntity>(e => e.PartitionKey == NotificationKeys.AllPartition))
        {
            list.Add(entity.ToSubscription());
        }

        return list;
    }
}
