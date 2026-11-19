# Hardhead night location and start time — design

## Goal

A Hardhead night gets two new fields: where it is held (location) and when it starts. Both show in the
"Næsta harðhausakvöld" box and in the 09:00 reminder: `Harðhaus #250 er í kvöld kl. 20:00 hjá Jónsi · Bláskógar 3`.

## Storage

| Field | Where | Why |
|---|---|---|
| Location | A `rep_Text` row per night, typed by the `gen_Type` row with `Shortcode = 'REP_T_LOCATION'` | Same as the night's other text fields (summary, movie, YouTube). Code looks the type up by shortcode, as ratings do, so no type ID is hard-coded. |
| Start time | The time part of `rep_Event.Date` | Already a datetime. 00:00 means no start time is set. |

### One-time database setup (owner)

Create the type. This copies `ParentId` and `GroupType` from the YouTube text type (208) so it sits with the other night text types:

```sql
INSERT INTO gen_Type (Name, Description, ParentId, Shortcode, GroupType, Inserted, InsertedBy)
SELECT N'Staðsetning', N'Staðsetning harðhausakvölds', ParentId, 'REP_T_LOCATION', GroupType, GETDATE(), <your user ID>
FROM   gen_Type
WHERE  Id = 208;
```

If `gen_Type.Id` isn't an identity column, add an unused `Id`. Until the row exists, nights load and save as before. Only saving a non-empty location fails, with "gen_Type with Shortcode REP_T_LOCATION is missing".

Check that the `UpdateHardhead` stored procedure stores `@hhDate` as given and doesn't cast it to `date`, which would drop the start time:

```sql
SELECT OBJECT_DEFINITION(OBJECT_ID('UpdateHardhead'));
```

## Behaviour

- **Saving** (`PUT /api/hardhead/{id}`, `HardheadInteractor.SaveHardheadAsync`):
  - `Location`: null leaves it as it is; empty or whitespace clears it; otherwise it's trimmed and saved, at most 100 characters. It's written only when changed.
  - `Date` is kept to the minute.
- **Edit form:** the date picker also picks a time in 15-minute steps, and there's a location text box. The form sends the date as wall-clock time (`2026-11-20T20:00:00`, no zone), because a JSON `Date` is UTC and would shift the time for anyone editing from outside Iceland.
- **API output:** `location`, `startTime` and `startTimeString` (`"20:00"` or null) are added. `dateString` now shows the full date for a future night once a start time is set. Without one it still shows only the month, since the day may be the placeholder a new night is created with.
- **Reminder:** the text adds ` kl. HH:mm` and ` · {location}` when they're set.
