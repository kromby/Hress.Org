using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.Notifications.UseCases;

public enum PushSendResult { Sent, Gone, Failed }

public interface IPushSender
{
    Task<PushSendResult> SendAsync(PushSubscription subscription, PushPayload payload);
}
