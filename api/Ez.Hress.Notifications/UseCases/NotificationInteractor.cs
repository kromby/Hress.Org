using Ez.Hress.Notifications.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.UseCases;

public class NotificationInteractor(
    INotificationDataAccess notifications,
    INotificationSeenDataAccess seen,
    IPushSubscriptionDataAccess subscriptions,
    IPushQueue queue,
    ILogger<NotificationInteractor> log)
{
    public const int TitleMaxLength = 100;

    public async Task<Notification> NotifyNewsCreatedAsync(int newsID, string newsTitle)
    {
        log.LogInformation("[{Class}.{Method}] Creating notification for news {NewsID}", nameof(NotificationInteractor), nameof(NotifyNewsCreatedAsync), newsID);
        if (newsID <= 0)
        {
            throw new ArgumentException("News ID must be positive", nameof(newsID));
        }

        var title = newsTitle ?? "";
        if (title.Length > TitleMaxLength)
        {
            title = title[..(TitleMaxLength - 1)] + "…";
        }

        var saved = await notifications.SaveAsync(new Notification
        {
            Type = NotificationTypes.News,
            Title = title,
            Link = $"/news/{newsID}",
            RecipientUserID = null,
            Created = DateTime.UtcNow
        });
        await queue.EnqueueAsync(saved.ID);
        return saved;
    }

    public async Task<NotificationList> GetNotificationsAsync(int userID, int top = 50)
    {
        log.LogInformation("[{Class}.{Method}] Getting notifications for user {UserID}", nameof(NotificationInteractor), nameof(GetNotificationsAsync), userID);
        var lastSeen = await seen.GetLastSeenAsync(userID);
        var items = await notifications.GetLatestAsync(userID, top);
        return new NotificationList { LastSeen = lastSeen, Notifications = items };
    }

    public async Task<bool> HasUnreadAsync(int userID)
    {
        log.LogInformation("[{Class}.{Method}] Checking unread for user {UserID}", nameof(NotificationInteractor), nameof(HasUnreadAsync), userID);
        var lastSeen = await seen.GetLastSeenAsync(userID);
        var latest = await notifications.GetLatestAsync(userID, 1);
        if (latest.Count == 0)
        {
            return false;
        }
        return lastSeen == null || latest[0].Created > lastSeen.Value;
    }

    /// <summary>
    /// Marks notifications as seen up to <paramref name="upTo"/>, the newest notification the member was shown.
    /// Without it, everything up to now is marked seen. Never moves the marker backwards or into the future.
    /// </summary>
    public async Task MarkSeenAsync(int userID, DateTime? upTo = null)
    {
        log.LogInformation("[{Class}.{Method}] Marking notifications seen for user {UserID}", nameof(NotificationInteractor), nameof(MarkSeenAsync), userID);
        var now = DateTime.UtcNow;
        var seenAt = upTo.HasValue && upTo.Value.ToUniversalTime() < now ? upTo.Value.ToUniversalTime() : now;

        var current = await seen.GetLastSeenAsync(userID);
        if (current.HasValue && current.Value >= seenAt)
        {
            return;
        }
        await seen.SetLastSeenAsync(userID, seenAt);
    }

    private const int MaxEndpointLength = 2048;

    // Hosts of the browser push services (Chrome/Edge-Chromium/Opera, Firefox, Windows, Safari).
    // A subdomain of one of these also matches, since WNS and Apple use per-region hosts.
    private static readonly string[] PushServiceHosts =
    [
        "fcm.googleapis.com",
        "android.googleapis.com",
        "push.services.mozilla.com",
        "notify.windows.com",
        "push.apple.com",
    ];

    private static bool IsKnownPushService(Uri endpoint)
    {
        var host = endpoint.IdnHost.ToLowerInvariant();
        return PushServiceHosts.Any(h => host == h || host.EndsWith("." + h, StringComparison.Ordinal));
    }

    public async Task SaveSubscriptionAsync(PushSubscription subscription)
    {
        log.LogInformation("[{Class}.{Method}] Saving push subscription for user {UserID}", nameof(NotificationInteractor), nameof(SaveSubscriptionAsync), subscription.UserID);
        if (string.IsNullOrWhiteSpace(subscription.Endpoint)
            || string.IsNullOrWhiteSpace(subscription.P256dh)
            || string.IsNullOrWhiteSpace(subscription.Auth))
        {
            throw new ArgumentException("Endpoint, P256dh and Auth are required", nameof(subscription));
        }

        if (subscription.Endpoint.Length > MaxEndpointLength
            || !Uri.TryCreate(subscription.Endpoint, UriKind.Absolute, out var endpointUri)
            || endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Endpoint must be an absolute https URL of at most 2048 characters", nameof(subscription));
        }

        if (!IsKnownPushService(endpointUri))
        {
            throw new ArgumentException("Endpoint must belong to a known browser push service", nameof(subscription));
        }

        subscription.Created = DateTime.UtcNow;
        await subscriptions.UpsertAsync(subscription);
    }

    public async Task RemoveSubscriptionAsync(int userID, string endpoint)
    {
        log.LogInformation("[{Class}.{Method}] Removing push subscription for user {UserID}", nameof(NotificationInteractor), nameof(RemoveSubscriptionAsync), userID);
        var stored = await subscriptions.GetAsync(endpoint);
        if (stored == null || stored.UserID != userID)
        {
            return;
        }
        await subscriptions.DeleteAsync(endpoint);
    }

    public async Task<bool> IsSubscribedAsync(int userID, string endpoint)
    {
        log.LogInformation("[{Class}.{Method}] Checking push subscription for user {UserID}", nameof(NotificationInteractor), nameof(IsSubscribedAsync), userID);
        var stored = await subscriptions.GetAsync(endpoint);
        return stored?.UserID == userID;
    }
}
