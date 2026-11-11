using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Notifications;

public record VapidPublicKey(string Value);

public class SubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public SubscriptionKeys? Keys { get; set; }
}

public class SubscriptionKeys
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class UnsubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
}

public class NotificationFunctions(
    AuthenticationInteractor authenticationInteractor,
    NotificationInteractor notificationInteractor,
    VapidPublicKey vapidPublicKey,
    ILogger<NotificationFunctions> log)
{
    private static readonly JsonSerializerOptions RequestJsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Function("notificationsList")]
    public Task<IActionResult> RunList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications")] HttpRequest req)
        => Execute(req, nameof(RunList), async userID =>
        {
            var list = await notificationInteractor.GetNotificationsAsync(userID);
            return new OkObjectResult(list);
        });

    [Function("notificationsSeen")]
    public Task<IActionResult> RunSeen(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/seen")] HttpRequest req)
        => Execute(req, nameof(RunSeen), async userID =>
        {
            await notificationInteractor.MarkSeenAsync(userID);
            return new NoContentResult();
        });

    [Function("notificationsStatus")]
    public Task<IActionResult> RunStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/status")] HttpRequest req)
        => Execute(req, nameof(RunStatus), async userID =>
        {
            var hasUnread = await notificationInteractor.HasUnreadAsync(userID);
            return new OkObjectResult(new { hasUnread });
        });

    [Function("notificationsVapidKey")]
    public IActionResult RunVapidKey(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/vapid-public-key")] HttpRequest req)
    {
        // Anonymous in the sense that no JWT is checked.
        if (string.IsNullOrWhiteSpace(vapidPublicKey.Value))
        {
            log.LogWarning("[{Class}.{Method}] VAPID public key is not configured", nameof(NotificationFunctions), nameof(RunVapidKey));
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }

        return new OkObjectResult(new { publicKey = vapidPublicKey.Value });
    }

    [Function("notificationsSubscribe")]
    public Task<IActionResult> RunSubscribe(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/subscriptions")] HttpRequest req)
        => Execute(req, nameof(RunSubscribe), async userID =>
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<SubscribeRequest>(body, RequestJsonOptions);
            if (request == null)
            {
                return new BadRequestResult();
            }

            await notificationInteractor.SaveSubscriptionAsync(new PushSubscription
            {
                Endpoint = request.Endpoint,
                P256dh = request.Keys?.P256dh ?? "",
                Auth = request.Keys?.Auth ?? "",
                UserID = userID,
                UserAgent = req.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null
            });
            return new NoContentResult();
        });

    [Function("notificationsUnsubscribe")]
    public Task<IActionResult> RunUnsubscribe(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "notifications/subscriptions")] HttpRequest req)
        => Execute(req, nameof(RunUnsubscribe), async userID =>
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<UnsubscribeRequest>(body, RequestJsonOptions);
            if (request == null || string.IsNullOrWhiteSpace(request.Endpoint))
            {
                return new BadRequestResult();
            }

            await notificationInteractor.RemoveSubscriptionAsync(userID, request.Endpoint);
            return new NoContentResult();
        });

    [Function("notificationsSubscribed")]
    public Task<IActionResult> RunSubscribed(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/subscriptions/current")] HttpRequest req)
        => Execute(req, nameof(RunSubscribed), async userID =>
        {
            string? endpoint = req.Query["endpoint"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return new BadRequestResult();
            }

            var subscribed = await notificationInteractor.IsSubscribedAsync(userID, endpoint);
            return new OkObjectResult(new { subscribed });
        });

    private async Task<IActionResult> Execute(HttpRequest req, string methodName, Func<int, Task<IActionResult>> action)
    {
        log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(NotificationFunctions), methodName);

        if (!AuthenticationUtil.GetAuthenticatedUserID(authenticationInteractor, req.Headers, log, out int userID))
        {
            log.LogInformation("[{Class}.{Method}] JWT is not valid!", nameof(NotificationFunctions), methodName);
            return new UnauthorizedResult();
        }

        try
        {
            return await action(userID);
        }
        catch (JsonException)
        {
            return new BadRequestResult();
        }
        catch (ArgumentException aex)
        {
            log.LogWarning("[{Class}.{Method}] Validation failed: {Message}", nameof(NotificationFunctions), methodName, aex.Message);
            return new BadRequestObjectResult(new { error = aex.Message });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "[{Class}.{Method}] Unhandled error", nameof(NotificationFunctions), methodName);
            throw;
        }
    }
}
