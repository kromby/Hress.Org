using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Notifications.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases;

/// <summary>Morning reminder, sent on the day of a Hardhead night to recent attendees.</summary>
public class HardheadReminderInteractor
{
    /// <summary>Who counts as a recent attendee: hosts and guests of nights in this many months before today.</summary>
    public const int AttendanceWindowMonths = 6;

    private readonly IHardheadDataAccess _hardheadDataAccess;
    private readonly NotificationInteractor _notificationInteractor;
    private readonly ILogger<HardheadReminderInteractor> _log;

    public HardheadReminderInteractor(IHardheadDataAccess hardheadDataAccess, NotificationInteractor notificationInteractor, ILogger<HardheadReminderInteractor> log)
    {
        _hardheadDataAccess = hardheadDataAccess;
        _notificationInteractor = notificationInteractor;
        _log = log;
    }

    /// <summary>
    /// Notifies recent attendees about every Hardhead night dated on <paramref name="today"/> whose date has
    /// been set. Safe to run more than once a day: nobody is notified twice about the same night.
    /// Returns how many notifications were queued.
    /// </summary>
    public async Task<int> NotifyTonightAsync(DateTime today)
    {
        var day = today.Date;
        _log.LogInformation("[{Class}.{Method}] Looking for Hardhead nights on {Day}", nameof(HardheadReminderInteractor), nameof(NotifyTonightAsync), day);

        var nights = (await _hardheadDataAccess.GetHardheadsOnDay(day)).Where(n => n.Date.Date == day).ToList();
        var scheduled = new List<HardheadNight>();
        foreach (var night in nights)
        {
            if (IsPlaceholderDate(night))
            {
                _log.LogInformation("[{Class}.{Method}] Skipping Hardhead night {HardheadID}: its date is still the placeholder", nameof(HardheadReminderInteractor), nameof(NotifyTonightAsync), night.ID);
                continue;
            }
            scheduled.Add(night);
        }

        if (scheduled.Count == 0)
        {
            _log.LogInformation("[{Class}.{Method}] No Hardhead night tonight", nameof(HardheadReminderInteractor), nameof(NotifyTonightAsync));
            return 0;
        }

        // Nights before today only: tonight's own guest list doesn't make someone a recent attendee.
        var attendees = await _hardheadDataAccess.GetAttendeeIDs(day.AddMonths(-AttendanceWindowMonths), day.AddSeconds(-1));

        var queued = 0;
        foreach (var night in scheduled)
        {
            queued += await _notificationInteractor.NotifyHardheadTonightAsync(night.ID, night.Number, night.Host.Username ?? "", day, attendees);
        }
        return queued;
    }

    /// <summary>
    /// When a host picks the next host, the next night is created dated on the last day of the following
    /// month (see HardheadInteractor.SaveHardheadAsync) until its host sets the real date. A night still on
    /// the last day of its month that has never been edited is treated as not scheduled yet.
    /// </summary>
    public static bool IsPlaceholderDate(HardheadNight night)
    {
        var isLastDayOfMonth = night.Date.Day == DateTime.DaysInMonth(night.Date.Year, night.Date.Month);
        return isLastDayOfMonth && !night.Updated.HasValue;
    }
}
