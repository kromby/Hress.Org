using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.NotificationFunctions;

public class HardheadReminderFunction(HardheadReminderInteractor reminderInteractor, DbConnectionInfo dbConnectionInfo, ILogger<HardheadReminderFunction> log)
{
    // 09:00 every day. Linux Function Apps run timers in UTC, which is also Iceland's time all year.
    // Keep in sync with NotificationInteractor.HardheadReminderTime.
    public const string Schedule = "0 0 9 * * *";

    [Function("hardheadReminder")]
    public async Task RunAsync([TimerTrigger(Schedule)] TimerInfo timer)
    {
        if (string.IsNullOrWhiteSpace(dbConnectionInfo.ConnectionString))
        {
            log.LogWarning("[HardheadReminderFunction.RunAsync] Skipped: setting 'Ez.Hress.Database.ConnectionString' is missing (see README.md)");
            return;
        }

        // Failures propagate so the run shows as failed. Running it again the same day is safe:
        // nobody gets a second notification about the same night.
        var queued = await reminderInteractor.NotifyTonightAsync(DateTime.UtcNow);

        log.LogInformation("[HardheadReminderFunction.RunAsync] Queued {Queued} Hardhead reminders", queued);
    }
}
