using Ez.Hress.Notifications.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Notifications.UseCases;

public class PushInteractor(
    INotificationDataAccess notifications,
    IPushSubscriptionDataAccess subscriptions,
    IPushSender sender,
    ILogger<PushInteractor> log)
{
    public const string NewsPushTitle = "Ný frétt";

    /// <summary>
    /// Sends the notification to the relevant devices. Per-device failures are counted, not thrown;
    /// storage failures propagate so the queue retries.
    /// </summary>
    public async Task<PushRunResult> SendAsync(string notificationID)
    {
        var notification = await notifications.GetAsync(notificationID);
        if (notification == null)
        {
            log.LogWarning("[PushInteractor.SendAsync] Notification {NotificationID} not found", notificationID);
            return new PushRunResult(0, 0, 0);
        }

        var all = await subscriptions.GetAllAsync();
        var targets = notification.RecipientUserID.HasValue
            ? all.Where(s => s.UserID == notification.RecipientUserID.Value).ToList()
            : all.ToList();

        var payload = new PushPayload(NewsPushTitle, notification.Title, notification.Link);
        int sent = 0, removed = 0, failed = 0;

        foreach (var subscription in targets)
        {
            PushSendResult result;
            try
            {
                result = await sender.SendAsync(subscription, payload);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "[PushInteractor.SendAsync] Sending to a device failed");
                result = PushSendResult.Failed;
            }

            switch (result)
            {
                case PushSendResult.Sent:
                    sent++;
                    break;
                case PushSendResult.Gone:
                    try
                    {
                        await subscriptions.DeleteAsync(subscription.Endpoint);
                        removed++;
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning(ex, "[PushInteractor.SendAsync] Removing a gone subscription failed");
                        failed++;
                    }
                    break;
                default:
                    failed++;
                    break;
            }
        }

        log.LogInformation("[PushInteractor.SendAsync] Notification {NotificationID}: sent {Sent}, removed {Removed}, failed {Failed}",
            notificationID, sent, removed, failed);
        return new PushRunResult(sent, removed, failed);
    }
}
