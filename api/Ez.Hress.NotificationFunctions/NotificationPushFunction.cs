using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.NotificationFunctions;

public class NotificationPushFunction(PushInteractor pushInteractor, ILogger<NotificationPushFunction> log)
{
    // Bound as a string and parsed by hand so the casing of the JSON keys never matters.
    [Function("notificationPush")]
    public async Task RunAsync([QueueTrigger(NotificationQueue.Name, Connection = "ContentStorage")] string body)
    {
        var message = NotificationPushMessage.Parse(body);

        // Storage failures propagate (queue retry). Per-device failures are only logged:
        // throwing would retry and re-send to devices that already got the push.
        var result = await pushInteractor.SendAsync(message.NotificationID);

        log.LogInformation("[NotificationPushFunction.RunAsync] Notification {NotificationID} result: {Result}", message.NotificationID, result);
    }
}
