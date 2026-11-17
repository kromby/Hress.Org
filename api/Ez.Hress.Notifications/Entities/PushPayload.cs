namespace Ez.Hress.Notifications.Entities;

/// <summary>Serialized camelCase as { title, body, link }.</summary>
public record PushPayload(string Title, string Body, string Link);
