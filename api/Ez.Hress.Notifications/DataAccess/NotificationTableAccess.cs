using Azure.Data.Tables;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.DataAccess;

public class NotificationTableAccess : INotificationDataAccess
{
    private readonly TableClient _table;
    private readonly ILogger<NotificationTableAccess> _logger;
    private readonly string _className;

    public NotificationTableAccess(BlobConnectionInfo connectionInfo, ILogger<NotificationTableAccess> logger)
    {
        _table = new TableClient(connectionInfo.ConnectionString, "Notifications");
        _table.CreateIfNotExists();
        _logger = logger;
        _className = nameof(NotificationTableAccess);
    }

    public async Task<Notification> SaveAsync(Notification notification)
    {
        _logger.LogInformation("[{Class}.{Method}] Save notification of type '{Type}'", _className, nameof(SaveAsync), notification.Type);

        var entity = new NotificationTableEntity(notification);
        await _table.AddEntityAsync(entity);

        notification.ID = entity.Id;
        return notification;
    }

    public async Task<Notification?> GetAsync(string id)
    {
        _logger.LogInformation("[{Class}.{Method}] Get notification '{ID}'", _className, nameof(GetAsync), id);

        var parts = id.Split('|', 2);
        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
        {
            _logger.LogWarning("[{Class}.{Method}] Malformed notification ID '{ID}'", _className, nameof(GetAsync), id);
            return null;
        }

        var response = await _table.GetEntityIfExistsAsync<NotificationTableEntity>(parts[0], parts[1]);
        return response.HasValue ? response.Value!.ToNotification() : null;
    }

    public async Task<IList<Notification>> GetLatestAsync(int userID, int top)
    {
        _logger.LogInformation("[{Class}.{Method}] Get latest {Top} notifications for user {UserID}", _className, nameof(GetLatestAsync), top, userID);

        var list = new List<Notification>();
        foreach (var partition in new[] { NotificationKeys.AllPartition, userID.ToString() })
        {
            // Row keys sort newest first, so the first 'top' rows of each partition are enough.
            var count = 0;
            await foreach (var entity in _table.QueryAsync<NotificationTableEntity>(e => e.PartitionKey == partition))
            {
                list.Add(entity.ToNotification());
                if (++count >= top)
                {
                    break;
                }
            }
        }

        return list.OrderByDescending(n => n.Created).Take(top).ToList();
    }
}
