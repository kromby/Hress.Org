using Ez.Hress.Hardhead.DataAccess;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Notifications.DataAccess;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

string Required(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Missing required setting '{key}'. Set it in the Function App configuration (see README.md).");
    }
    return value;
}

var contentStorage = Required("ContentStorage");
var vapidPublicKey = Required("Ez.Hress.Notifications.Vapid.PublicKey");
var vapidPrivateKey = Required("Ez.Hress.Notifications.Vapid.PrivateKey");
var vapidSubject = Required("Ez.Hress.Notifications.Vapid.Subject");
var vapidSettings = new VapidSettings(vapidPublicKey, vapidPrivateKey, vapidSubject);
vapidSettings.Validate();
// Optional so push for news keeps working without it; the Hardhead reminder skips its run while it's missing.
var dbConnectionString = Environment.GetEnvironmentVariable("Ez.Hress.Database.ConnectionString") ?? "";

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        // Remove the default Warning+ filter so Information logs reach Application Insights.
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
                options.Rules.Remove(defaultRule);
        });
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton(vapidSettings);
        // No redirects: a push service must answer itself, never forward the VAPID header elsewhere.
        services.AddHttpClient<IPushSender, WebPushSender>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton(new BlobConnectionInfo(contentStorage));
        services.AddSingleton<INotificationDataAccess, NotificationTableAccess>(sp => new NotificationTableAccess(sp.GetRequiredService<BlobConnectionInfo>(), sp.GetRequiredService<ILogger<NotificationTableAccess>>()));
        services.AddSingleton<IPushSubscriptionDataAccess, PushSubscriptionTableAccess>(sp => new PushSubscriptionTableAccess(sp.GetRequiredService<BlobConnectionInfo>(), sp.GetRequiredService<ILogger<PushSubscriptionTableAccess>>()));
        services.AddSingleton<PushInteractor>();

        // Hardhead reminder
        services.AddSingleton<INotificationSeenDataAccess, NotificationSeenTableAccess>(sp => new NotificationSeenTableAccess(sp.GetRequiredService<BlobConnectionInfo>(), sp.GetRequiredService<ILogger<NotificationSeenTableAccess>>()));
        services.AddSingleton<IPushQueue, PushQueueStorageAccess>(sp => new PushQueueStorageAccess(sp.GetRequiredService<BlobConnectionInfo>(), sp.GetRequiredService<ILogger<PushQueueStorageAccess>>()));
        services.AddSingleton<NotificationInteractor>();
        services.AddSingleton(new DbConnectionInfo(dbConnectionString));
        services.AddSingleton<IHardheadDataAccess, HardheadSqlAccess>();
        services.AddSingleton<HardheadReminderInteractor>();
    })
    .Build();

host.Run();
