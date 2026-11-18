# Hardhead night reminder — design

## Goal

On the morning of a Hardhead night, notify the people who come to Hardhead nights, using the
notification bell and web push from the news notifications work (2026-11-08-news-notifications-design.md).

## Decisions

| Question | Decision |
|---|---|
| When | 09:00 every day. Iceland is on UTC all year, so the timer runs on `0 0 9 * * *` (Linux Function Apps use UTC). |
| Who | Everyone who hosted (`rep_User.TypeId = 53`) or was a guest (`TypeId = 52`) at a night dated in the 6 months before today. Tonight's own guest list doesn't count. User 2646 is excluded, as in the guest counts. |
| Text | Bell and push body: `Harðhaus #N er í kvöld hjá {host}`. Push title: `Harðhausakvöld`. Bell label: `Harðhausakvöld ·`. Link: `/hardhead/{id}`. |
| Unset dates | A night's next host creates it dated on the last day of the following month (`HardheadInteractor.SaveHardheadAsync`), until its host sets the real date. A night still on the last day of its month with `rep_Event.Updated` null is treated as not scheduled and skipped. A host who really picks the last day and never edits the night again isn't notified. |

## Design

- **Where:** a timer function, `hardheadReminder`, in `Ez.Hress.NotificationFunctions`. Static Web App managed functions only support HTTP triggers.
- **`HardheadReminderInteractor.NotifyTonightAsync(today)`** (Ez.Hress.Hardhead): loads tonight's nights (`IHardheadDataAccess.GetHardheadsOnDay`) and drops placeholder dates. It loads recent attendees (`GetAttendeeIDs`) only when a night remains, then calls `NotificationInteractor.NotifyHardheadTonightAsync` for each night.
- **`NotificationInteractor.NotifyHardheadTonightAsync`** (Ez.Hress.Notifications): saves one notification per recipient (`RecipientUserID` set, so it appears only in that person's bell and goes only to their devices) and queues each for the existing `notificationPush` function.
- **Never twice:** these notifications carry a `Key` (`hardhead-{id}`) and are stamped at 09:00 on the night's date, so the table row key is the same on every run. `INotificationDataAccess.SaveIfNewAsync` returns null on a 409 conflict, and nothing is queued. A rerun after a failure fills in whoever was missed without sending anyone a second push.
- **Configuration:** the Function App needs `Ez.Hress.Database.ConnectionString` and access through the SQL firewall. Without the setting the reminder logs a warning and skips the run, so news push keeps working.

## Not included

- Start time and location in the text. These are planned as new night fields; the text can include them once they exist.
- Per-user opt-out of reminders.
