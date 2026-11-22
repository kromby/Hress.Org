# Ölrýnir notifications — design

Issues: #764 (beer added), #765 (comment), #766 (Friday top beer, later), #767 (throttling, later).
Builds on the news notifications (2026-11-08-news-notifications-design.md) and the Hardhead
reminder (2026-11-18-hardhead-night-reminder-design.md).

## Decisions

| Question | Decision |
|---|---|
| Beer added: who | Everyone except the member who added it. |
| Beer added: when | A new beer is created in the season, or a catalog beer is added to it. Re-adding a beer already in the season sends nothing. |
| Comment: who | Everyone who used Ölrýnir in the last 6 months (registered a beer, rated a beer, or posted in the Ölrýnir chat), minus the commenter. |
| Throttling | None for now (#767). |
| Friday (#766) | 09:00 Fridays, the current open season's highest average among beers with at least 3 ratings, to the same 6-month audience. |

## Design

- **Beer added** is one shared notification, like news, with a new optional `Notification.ExcludedUserID`.
  The bell list leaves it out for that member and `PushInteractor` skips their devices. Type `beer`.
  Push title `Nýr bjór í Ölrýni`, body `{name} · {producer}`, link `/beer/{id}`.
- **Comment** notifications are one per recipient (`RecipientUserID`), like the Hardhead reminder. Type
  `beer-chat`. Push title `Ölrýnir`, body `{username} um {beer}: {comment}` cut to 100 characters, link
  `/beer/{id}`.
- **Active members** are found by a BeerRater interactor that reads beers (`RegisteredBy`/`RegisteredAt`),
  ratings across seasons, and the distinct posters in the Beer Rater chat category since the cutoff. The
  Friday timer reuses it.
- **Best effort:** adding a beer or posting a comment never fails because of notifications. Failures and
  timeouts are logged, as in `NewsInteractor.CreateNewsAsync`.
- **Bell:** both types show the label `Ölrýnir ·` on `/notifications`.

## Not included

- Throttling busy chats (#767).
- Per-member opt-out.
