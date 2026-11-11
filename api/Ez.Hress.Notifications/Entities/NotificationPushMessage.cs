using System.Text.Json;

namespace Ez.Hress.Notifications.Entities;

/// <summary>Queue message written by PushQueueStorageAccess: {"notificationID":"&lt;id&gt;"}.</summary>
public record NotificationPushMessage(string NotificationID)
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Parses the queue body regardless of property-name casing.</summary>
    public static NotificationPushMessage Parse(string body)
    {
        NotificationPushMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<NotificationPushMessage>(body, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Queue message is not valid JSON", ex);
        }

        if (message == null || string.IsNullOrWhiteSpace(message.NotificationID))
        {
            throw new InvalidOperationException("Queue message has no notificationID");
        }
        return message;
    }
}
