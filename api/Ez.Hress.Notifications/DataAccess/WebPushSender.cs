using System.Net;
using System.Text.Json;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Logging;
using WebPushSubscription = Lib.Net.Http.WebPush.PushSubscription;
using PushSubscription = Ez.Hress.Notifications.Entities.PushSubscription;

namespace Ez.Hress.Notifications.DataAccess;

public class WebPushSender : IPushSender
{
    private const int TimeToLiveSeconds = 86400; // 24 hours
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly PushServiceClient _client;
    private readonly ILogger<WebPushSender> _log;

    public WebPushSender(VapidSettings vapid, HttpClient httpClient, ILogger<WebPushSender> log)
    {
        _log = log;
        _client = new PushServiceClient(httpClient)
        {
            DefaultAuthentication = new VapidAuthentication(vapid.PublicKey, vapid.PrivateKey) { Subject = vapid.Subject }
        };
    }

    public async Task<PushSendResult> SendAsync(PushSubscription subscription, PushPayload payload)
    {
        var target = new WebPushSubscription { Endpoint = subscription.Endpoint };
        target.SetKey(PushEncryptionKeyName.P256DH, subscription.P256dh);
        target.SetKey(PushEncryptionKeyName.Auth, subscription.Auth);

        var message = new PushMessage(JsonSerializer.Serialize(payload, JsonOptions)) { TimeToLive = TimeToLiveSeconds };

        try
        {
            await _client.RequestPushMessageDeliveryAsync(target, message);
            return PushSendResult.Sent;
        }
        catch (PushServiceClientException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
        {
            _log.LogInformation("[{Class}.{Method}] Subscription gone ({Status})", nameof(WebPushSender), nameof(SendAsync), ex.StatusCode);
            return PushSendResult.Gone;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "[{Class}.{Method}] Push delivery failed", nameof(WebPushSender), nameof(SendAsync));
            return PushSendResult.Failed;
        }
    }
}
