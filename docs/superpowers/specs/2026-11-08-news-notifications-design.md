# News notifications — design

Web push and a notification bell for new news posts. Related: issue #325 ("Web push - guide").

## Why

Events are often announced only as a news post on Hress.Org. Members who don't check the site
miss them. Notifications bring those announcements to members instead of waiting for them to
visit.

Success: when a news post announcing an event goes up, members who turned on push get it on
their phone within minutes, and everyone who logs in sees there's something new.

## Goals

- Every new news post creates a notification.
- Every logged-in member sees a bell in the header. A dot shows when something has arrived
  since they last opened the notifications page.
- A member can turn on push for the device they're using. After that, every new news post
  sends a push to that device, and tapping it opens the post.
- Push sending runs in a new, separate Function App, so posting news never waits on push
  services and failed sends are retried.

## Non-goals (for this version)

- Chat notifications (beer chat or any other chat). These come later. The data model leaves
  room for them (see "Notification" below), but nothing is built for them now.
- Notifications for edited news posts. Only newly created posts trigger one.
- Email or any channel other than web push and the bell.
- Per-event preferences ("only news of type X"). Push is simply on or off per device.
- Notifications for posts created anywhere other than the new API's `POST /news`
  (legacy systems are not touched).

## Anti-goals

- Posting a news item must never fail or slow down because of push. If storing the bell
  notification or queueing the push fails, the news post is still created. Log the error.
- No permission prompt on page load. The browser permission prompt only appears after the
  member clicks "Turn on".
- Don't nag. No front page prompts or pop-ups asking members to turn on push.

## Decisions made with the owner

- **Channels:** web push plus an in-site notification bell.
- **Events:** new news posts only. Every new post notifies every member.
- **Author:** the author may receive their own notification. There's no need to exclude
  them, but it's fine to if that's trivial.
- **Timing:** a delay of seconds, even minutes, is acceptable.
- **Push tap:** opens that news post (`/news/{id}`).
- **Bell:** shown to every logged-in member, whether or not they've turned on push. Not shown
  when logged out.
- **Bell click:** goes to a notifications page (`/notifications`) with the full history, not
  a dropdown.
- **Read state:** opening the notifications page clears the dot. There is no per-item read
  state.
- **Turning on push:** a banner at the top of the notifications page, "Get a notification on
  this device when new news is posted", with a Turn on button. When on, it shrinks to "On for
  this device · Turn off". There's no toggle in the profile and no prompt elsewhere.
- **Sending:** a brand-new, separate Azure Function App. The owner creates the Azure resources.
- **Runtime (decided after the builder check):** move everything to .NET 10. .NET 9 went out of
  support on 2026-11-10, and every `api/` project targets `net9.0` today. The first step of this
  work upgrades the whole `api/` solution to `net10.0` (packages included) and the Static Web
  App's `apiRuntime` in `staticwebapp.config.json` to `dotnet-isolated:10.0`. The new Function
  App and the notifications module are built on .NET 10. If the Static Web App managed API turns
  out not to support .NET 10 yet, stop and raise it with the owner rather than splitting
  runtimes. (CLAUDE.md's ".NET 8" is out of date; update it as part of the upgrade.)
- **Icons (decided after the builder check):** upscale the existing 110×110 `public/logo.png`
  to 192×192 and 512×512 for now. A sharper logo will replace them later.

## Calls made in the design (not explicitly agreed, but presented and accepted)

- **Unread marker:** each member has one `LastSeen` time for the notifications page.
  Notifications newer than it are unread. Opening the page sets it to now. A member with no
  `LastSeen` treats everything as unread, and the list only contains notifications from launch
  onward.
- **Highlighting:** the page highlights items that were unread when the member arrived. The
  page reads the previous `LastSeen` before updating it.
- **News notifications are shared:** there's one notification per news post, not one copy
  per member. Each notification has a type (`news`) and the link it opens. An optional
  recipient field (empty = everyone) leaves room for chat notifications aimed at specific
  members later. This version only writes "everyone" notifications.
- **Push text:** title "Ný frétt", body = the news post's title. The UI text is Icelandic,
  matching the site.
- **iPhone:** iOS only supports web push once the site has been added to the home screen
  (iOS 16.4+). On iOS when the site isn't running from the home screen, the banner explains
  how to add it instead of showing a Turn on button. `public/manifest.json` gets 192×192 and
  512×512 PNG icons so the site installs properly.
- **Blocked permission:** if the browser has blocked notifications for the site, the banner
  says so and explains how to unblock them, because the site can't ask again.
- **Unsupported browsers:** if the browser has no push support, the banner is hidden. The
  bell and the page still work.
- **Subscriptions belong to the member who turned push on (decided after the builder check):**
  a subscription is stored against that member's user ID. Signing out turns push off for that
  device: before the token is cleared, the SPA calls `DELETE /subscriptions` and unsubscribes
  the browser (best effort; a failure must not block sign-out). The banner shows "On" only if
  the browser has a subscription *and* the server says that endpoint belongs to the current
  member (`GET /subscriptions/current?endpoint=…`, see below). If the browser has a
  subscription the server doesn't know about (for example, pruned after a 404/410, or a token
  that expired without a sign-out), the banner shows "Off". Turning it on again re-uses or
  replaces the browser subscription and upserts it.
- **Flow:** the existing API stores the bell notification directly, then queues a push job.
  The bell keeps working even if the push Function App is down.
- **Code location:** the new Function App lives in the same `api/` solution as a new project,
  sharing a new notifications domain module with the existing API.

## How it works

```
Member posts news (SPA → POST /api/news, Static Web App managed API)
  └─ NewsInteractor.CreateNewsAsync
       ├─ saves news (existing)
       └─ NotificationInteractor.NotifyNewsCreatedAsync(news)   ← new, failures logged and swallowed
            ├─ stores Notification { type: news, title, link: /news/{id}, recipient: everyone }
            └─ enqueues message { notificationId } on Storage queue "notification-push"

New Function App (Ez.Hress.NotificationFunctions)
  └─ QueueTrigger "notification-push"
       └─ PushInteractor.SendAsync(notificationId)
            ├─ loads notification
            ├─ loads all push subscriptions (for "everyone")
            ├─ sends web push (VAPID) to each; 404/410 → delete that subscription
            └─ other failures: log; a failure of the whole run is retried by the queue (poison queue after max retries)

Browser
  ├─ header bell: GET /api/notifications/status → { hasUnread }
  ├─ /notifications page: GET /api/notifications (list + previous LastSeen), then marks seen
  ├─ banner Turn on: Notification.requestPermission() → pushManager.subscribe(VAPID public key)
  │    → POST /api/notifications/subscriptions
  └─ service worker (public/sw.js): on push → showNotification; on click → open /news/{id}
```

## Backend

Follows `ARCHITECTURE_RULES.md` and the module layout in `CLAUDE.md`.

### New domain module: `Ez.Hress.Notifications`

- **Entities**
  - `Notification`: ID, Type (`news`), Title, Link, RecipientUserId (null = everyone),
    Created.
  - `PushSubscription`: UserId, Endpoint, P256dh key, Auth key, Created, plus the UserAgent so
    a member can tell their devices apart later. A subscription is identified by its endpoint.
  - `NotificationSeen`: UserId, LastSeen.
- **Use cases**
  - `NotificationInteractor`: create a notification for a news post and queue its push; list
    notifications for a user (everyone + recipient = user, newest first); get unread status;
    mark seen; add and remove a subscription; check whether this device's endpoint is
    subscribed.
  - `PushInteractor` (used by the new Function App): send a notification to its recipients'
    subscriptions and prune dead ones.
  - Data access interfaces for notifications, subscriptions, seen marker and the push queue,
    plus a push sender interface so `PushInteractor` is unit-testable.
- **Data access:** Azure Table Storage in the existing content storage account (like the beer
  and album modules). Builder chooses the keys. Suggested: `Notifications` (newest-first
  RowKey via inverted ticks), `PushSubscriptions` (one partition, RowKey = hash of the endpoint,
  UserId as a property, so that upserting by endpoint moves a device between members),
  `NotificationSeen` (PartitionKey = userId). Queue via `Azure.Storage.Queues`.
- **Push library:** builder's choice of a maintained .NET web push library that supports VAPID
  (for example `WebPush` or `Lib.Net.Http.WebPush`).

### Changes to the existing API (`Ez.Hress.FunctionsApi`, Static Web App)

- `NewsInteractor.CreateNewsAsync` calls the notification interactor after the news is saved.
  Notification failures are caught and logged, and the news response is unaffected.
- New HTTP functions (all JWT-authenticated with `X-Custom-Authorization`, except the public
  key):
  - `GET  /api/notifications`: list for the current user, plus the `LastSeen` before this
    visit. Paging is the builder's choice; the first 50 is fine to start.
  - `POST /api/notifications/seen`: set `LastSeen` to now.
  - `GET  /api/notifications/status`: `{ hasUnread }` for the bell.
  - `GET  /api/notifications/vapid-public-key`: the public key.
  - `POST /api/notifications/subscriptions`: save this browser's subscription for the current
    user. Upsert by endpoint, so re-subscribing or switching user on the same device updates
    it.
  - `DELETE /api/notifications/subscriptions`: remove by endpoint (in the body).
  - `GET  /api/notifications/subscriptions/current?endpoint=…`: whether this endpoint is
    subscribed for the current member (`{ subscribed: bool }`). The banner uses it.
- DI registrations in `Program.cs`, singletons as usual.

### New Function App project: `Ez.Hress.NotificationFunctions`

- .NET 10 isolated worker (after the solution upgrade above). Added to
  `Ez.Hress.FunctionsApi.sln`.
- One function: a queue trigger on `notification-push` that calls `PushInteractor`.
- Its own `Program.cs` with DI for the notifications module only.

### Tests

xUnit + Moq in `Ez.Hress.UnitTest`, matching existing interactor tests:
- Creating news calls the notifier. A notifier failure doesn't fail creation.
- Unread status relative to `LastSeen` (including no `LastSeen`).
- Subscription upsert and remove.
- `PushInteractor` sends to every subscription and deletes the ones that return 404/410.

## Frontend

- **Service worker** `public/sw.js`, registered in `src/index.js` in place of
  `serviceWorker.unregister()`. It only handles `push` (show notification with title, body,
  icon, link) and `notificationclick` (focus an open Hress.Org tab and navigate it, or open a
  new one at the link). No offline caching. Delete the unused CRA `src/serviceWorker.js`.
- **Bell** in the header's `nav.main` in `src/pages/App.js`, before Search, styled like the
  existing icons. Only rendered when logged in. It shows a dot when `hasUnread`, and checks the
  status on load and on route changes. Links to `/notifications`.
- **Notifications page** `/notifications`: the push banner on top, then the list (title,
  "Ný frétt", relative time). Items unread on arrival are highlighted. Clicking an item opens
  its link. Opening the page posts `seen` and clears the bell dot right away.
- **Push banner states:** unsupported (hidden) · iOS not installed (add-to-home-screen
  instructions) · blocked (how to unblock) · off (Turn on) · on for this device (Turn off).
  "On" means the browser has a subscription and the server confirms it belongs to the current
  member (see "Subscriptions belong to the member" above).
- **Sign-out:** the existing sign-out paths (`src/pages/frame/menu.js`, `src/pages/App.js`)
  turn push off for the device before clearing the token, best effort.
- **Hooks** in `src/hooks/` (TypeScript, React Query, Axios with the auth header), following
  existing hooks such as `useCreateNews.ts`.
- **Manifest:** 192×192 and 512×512 PNG icons upscaled from `public/logo.png` (see "Icons"),
  plus `apple-touch-icon` in `public/index.html`.
- **Static Web App config:** `/sw.js` served with `cache-control: no-cache` so updates reach
  members.
- Mockups from the design session (bell page option B, banner option A) are in
  `.superpowers/brainstorm/` (not committed).

## Azure setup (owner does this; the plan should list it as a checklist)

- Generate a VAPID key pair (the plan should include how) and choose a contact `mailto:` for
  the VAPID subject.
- Create the new Function App (consumption plan is fine) on .NET 10 isolated.
- Create the `notification-push` queue in the content storage account (or let the code create
  it if missing).
- Settings:
  - **Static Web App** (see the memory note: API settings live on the SWA): VAPID public key,
    queue connection (reuse the content storage connection string).
  - **New Function App:** storage connection for the queue trigger and tables, VAPID public +
    private key, VAPID subject.
- A deploy pipeline for the new Function App, in the same style as the existing CI. The plan
  should propose it.

## Left to the builder

- Table keys and schema details, paging, setting names, queue message shape.
- Which web push library to use.
- How the bell checks status (on navigation is enough; no polling or live updates needed).
- Exact Icelandic wording of banner states and the empty list.
- Whether to skip notifying the author (only if trivial).
