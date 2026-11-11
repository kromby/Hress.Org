using System.Text.Json;
using Azure.Storage.Queues;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.DataAccess;

public class PushQueueStorageAccess : IPushQueue
{
    private readonly QueueClient _queue;
    private readonly EnsureCreatedOnce _ensureCreated;
    private readonly ILogger<PushQueueStorageAccess> _logger;
    private readonly string _className;

    public PushQueueStorageAccess(BlobConnectionInfo connectionInfo, ILogger<PushQueueStorageAccess> logger)
    {
        // The Functions queue trigger expects base64 messages; plain text ends up in the poison queue.
        _queue = new QueueClient(connectionInfo.ConnectionString, NotificationQueue.Name,
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        _ensureCreated = new EnsureCreatedOnce(() => _queue.CreateIfNotExistsAsync());
        _logger = logger;
        _className = nameof(PushQueueStorageAccess);
    }

    public async Task EnqueueAsync(string notificationID)
    {
        _logger.LogInformation("[{Class}.{Method}] Enqueue push for notification '{ID}'", _className, nameof(EnqueueAsync), notificationID);
        await _ensureCreated.RunAsync();

        var body = JsonSerializer.Serialize(new { notificationID });
        await _queue.SendMessageAsync(body);
    }
}
