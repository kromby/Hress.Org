using Azure;
using Azure.Data.Tables;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.DataAccess;

public class NotificationTableAccess : INotificationDataAccess
{
    private readonly TableClient _table;
    private readonly EnsureCreatedOnce _ensureCreated;
    private readonly ILogger<NotificationTableAccess> _logger;
    private readonly string _className;

    public NotificationTableAccess(BlobConnectionInfo connectionInfo, ILogger<NotificationTableAccess> logger)
    {
        _table = new TableClient(connectionInfo.ConnectionString, "Notifications");
        _ensureCreated = new EnsureCreatedOnce(() => _table.CreateIfNotExistsAsync());
        _logger = logger;
        _className = nameof(NotificationTableAccess);
    }

    public async Task<Notification> SaveAsync(Notification notification)
    {
        _logger.LogInformation("[{Class}.{Method}] Save notification of type '{Type}'", _className, nameof(SaveAsync), notification.Type);
        await _ensureCreated.RunAsync();

        var entity = new NotificationTableEntity(notification);
        await _table.AddEntityAsync(entity);

        notification.ID = entity.Id;
        return notification;
    }

    public async Task<Notification?> SaveIfNewAsync(Notification notification)
    {
        _logger.LogInformation("[{Class}.{Method}] Save notification of type '{Type}' with key '{Key}'", _className, nameof(SaveIfNewAsync), notification.Type, notification.Key);
        if (string.IsNullOrWhiteSpace(notification.Key))
        {
            throw new ArgumentException("A notification saved with SaveIfNewAsync needs a Key", nameof(notification));
        }
        await _ensureCreated.RunAsync();

        var entity = new NotificationTableEntity(notification);
        try
        {
            await _table.AddEntityAsync(entity);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            _logger.LogInformation("[{Class}.{Method}] Notification '{ID}' already exists", _className, nameof(SaveIfNewAsync), entity.Id);
            return null;
        }

        notification.ID = entity.Id;
        return notification;
    }

    public async Task<Notification?> GetAsync(string id)
    {
        _logger.LogInformation("[{Class}.{Method}] Get notification '{ID}'", _className, nameof(GetAsync), id);
        await _ensureCreated.RunAsync();

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
        await _ensureCreated.RunAsync();

        var list = new List<Notification>();
        foreach (var partition in new[] { NotificationKeys.AllPartition, userID.ToString() })
        {
            list.AddRange(await TakeVisibleAsync(_table.QueryAsync<NotificationTableEntity>(e => e.PartitionKey == partition), userID, top));
        }

        return list.OrderByDescending(n => n.Created).Take(top).ToList();
    }

    /// <summary>
    /// The first <paramref name="top"/> rows of a partition that the member may see. Row keys sort newest first,
    /// so that's enough. Rows that exclude the member are skipped without counting, so they don't shorten the list.
    /// </summary>
    internal static async Task<IList<Notification>> TakeVisibleAsync(IAsyncEnumerable<NotificationTableEntity> rows, int userID, int top)
    {
        var list = new List<Notification>();
        if (top <= 0)
        {
            return list;
        }

        await foreach (var entity in rows)
        {
            if (entity.ExcludedUserID == userID)
            {
                continue;
            }

            list.Add(entity.ToNotification());
            if (list.Count >= top)
            {
                break;
            }
        }
        return list;
    }
}
