using System.Text.Json;

namespace Ez.Hress.NotificationFunctions;

/// <summary>Queue message written by PushQueueStorageAccess: {"notificationID":"&lt;id&gt;"}.</summary>
public record NotificationPushMessage(string NotificationID)
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Parses the queue body regardless of property-name casing.</summary>
    public static NotificationPushMessage Parse(string body)
    {
        var message = JsonSerializer.Deserialize<NotificationPushMessage>(body, Options);
        if (message == null || string.IsNullOrWhiteSpace(message.NotificationID))
        {
            throw new InvalidOperationException("Queue message has no notificationID");
        }
        return message;
    }
}
