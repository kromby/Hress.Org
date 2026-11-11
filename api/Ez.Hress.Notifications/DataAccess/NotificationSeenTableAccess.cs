using Azure.Data.Tables;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.DataAccess;

public class NotificationSeenTableAccess : INotificationSeenDataAccess
{
    private const string NewsRowKey = "news";

    private readonly TableClient _table;
    private readonly ILogger<NotificationSeenTableAccess> _logger;
    private readonly string _className;

    public NotificationSeenTableAccess(BlobConnectionInfo connectionInfo, ILogger<NotificationSeenTableAccess> logger)
    {
        _table = new TableClient(connectionInfo.ConnectionString, "NotificationSeen");
        _table.CreateIfNotExists();
        _logger = logger;
        _className = nameof(NotificationSeenTableAccess);
    }

    public async Task<DateTime?> GetLastSeenAsync(int userID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get last seen for user {UserID}", _className, nameof(GetLastSeenAsync), userID);

        var response = await _table.GetEntityIfExistsAsync<TableEntity>(userID.ToString(), NewsRowKey);
        if (!response.HasValue)
        {
            return null;
        }

        var lastSeen = response.Value!.GetDateTimeOffset("LastSeen");
        return lastSeen?.UtcDateTime;
    }

    public async Task SetLastSeenAsync(int userID, DateTime seen)
    {
        _logger.LogInformation("[{Class}.{Method}] Set last seen for user {UserID}", _className, nameof(SetLastSeenAsync), userID);

        var entity = new TableEntity(userID.ToString(), NewsRowKey)
        {
            ["LastSeen"] = new DateTimeOffset(DateTime.SpecifyKind(seen, DateTimeKind.Utc))
        };
        await _table.UpsertEntityAsync(entity);
    }
}
