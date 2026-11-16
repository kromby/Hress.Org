# News Notifications Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Every new news post shows up behind a bell on Hress.Org for every logged-in member, and as a web push on every device where a member turned push on.

**Architecture:** A new `Ez.Hress.Notifications` domain module (Table Storage + Storage queue) is shared by the existing Static Web App API and a new queue-triggered Function App, `Ez.Hress.NotificationFunctions`. Creating a news post stores a notification and queues a push job. The new app sends VAPID web pushes. The SPA gets a service worker, a header bell, and a `/notifications` page with a push on/off banner.

**Tech Stack:** .NET 10 isolated Azure Functions, Azure.Data.Tables, Azure.Storage.Queues, Lib.Net.Http.WebPush, xUnit + Moq. React 19, TanStack Query v5, Axios, Jest + Testing Library.

**Spec:** `docs/superpowers/specs/2026-11-08-news-notifications-design.md`. Read it before starting any task.

## Global Constraints

- All `api/` projects target `net10.0`. `staticwebapp.config.json` `apiRuntime` is `dotnet-isolated:10.0`.
- Only newly created news posts notify. Editing a post never does.
- Creating a news post must never fail because of notifications. Catch, log, continue.
- No permission prompt except from the member clicking "Turn on".
- Push title is exactly `Ný frétt`. Push body is the news post's title. Tapping opens `/news/{id}`.
- The bell is only rendered for logged-in members. Opening `/notifications` clears the dot.
- The list only contains notifications created after launch (nothing is backfilled).
- Backend conventions follow `ARCHITECTURE_RULES.md`: interactors have no interfaces, data access interfaces live in `UseCases/`, table entities live in `DataAccess/`, logging uses `[{Class}.{Method}]`, DI registrations are singletons, and auth uses the `X-Custom-Authorization: token <jwt>` header via `AuthenticationUtil.GetAuthenticatedUserID`.
- Frontend: TypeScript for new files. Prettier style (double quotes, es5 trailing commas, 2 spaces, semicolons). UI text is Icelandic.
- Queue name: `notification-push`. Table names: `Notifications`, `NotificationSeen`, `PushSubscriptions`.
- Settings: `Ez.Hress.Notifications.Vapid.PublicKey` (SWA + new app), `Ez.Hress.Notifications.Vapid.PrivateKey` and `Ez.Hress.Notifications.Vapid.Subject` (new app only), `ContentStorage` (new app: content storage connection string). The SWA keeps using `Ez.Hress.Shared.ContentStorage.ConnectionString`.

## Review Focus

1. **Endpoint URLs as table keys:** push endpoints contain `/`, `?` and `#`, which Azure Table keys forbid. Keys must be a hash of the endpoint, never the URL. (Test: Task 3.)
2. **Same device, different member:** when member B turns on push on a device member A used, the device belongs to B only. It's one row, not two, and pushes aren't sent twice. (Test: Task 2, upsert by endpoint.)
3. **One dead device among many:** a 404/410 or a network error for one subscription must not stop sends to the others. (Test: Task 5.)
4. **Long or odd news titles:** a 500-character title must still fit the push payload. Truncate to 100 characters with `…`. (Test: Task 2.)
5. **Sign-out when the API is down or slow:** turning push off at sign-out is best effort. Sign-out completes within 3 seconds even if the call fails or hangs. (Test: Task 10.)

---

### Task 1: Upgrade the API solution to .NET 10

**Files:**
- Modify: every `api/*/*.csproj` (11 projects): `net9.0` → `net10.0`. Bump every `9.0.x` Microsoft package (`Microsoft.Extensions.Logging.Abstractions`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson`, `System.Configuration.ConfigurationManager`, and any others) to the latest `10.0.x`.
- Modify: `staticwebapp.config.json`: `"apiRuntime": "dotnet-isolated:10.0"`
- Modify: `api/README.md` line 3: ".NET 9" → ".NET 10".
- Modify (outside this repo, not committed): the workspace `CLAUDE.md` at `/home/kromby/source/Hress/CLAUDE.md`. Change the Hress.Org API's ".NET 8" mentions to ".NET 10".

- [ ] **Step 1: Confirm Static Web Apps supports `dotnet-isolated:10.0` for managed functions** (Microsoft Learn: "Supported languages and runtimes in Azure Static Web Apps"). If it doesn't, **stop and report to the owner.** Don't split runtimes.
- [ ] **Step 2: Make the changes above.**
- [ ] **Step 3: Build and test**

Run: `cd api && dotnet build Ez.Hress.FunctionsApi.sln && dotnet test Ez.Hress.FunctionsApi.sln`
Expected: build succeeds with no new warnings about the target framework, and all tests pass.

- [ ] **Step 4: Commit**: `chore(api): upgrade to .NET 10`

---

### Task 2: Notifications domain: entities and `NotificationInteractor`

**Files:**
- Create: `api/Ez.Hress.Notifications/Ez.Hress.Notifications.csproj`: net10.0, nullable, references `Ez.Hress.Shared`, packages `Azure.Data.Tables` (same version as other modules), `Azure.Storage.Queues`, `Microsoft.Extensions.Logging.Abstractions`. `InternalsVisibleTo Ez.Hress.UnitTest`. Add it to `Ez.Hress.FunctionsApi.sln`, and reference it from `Ez.Hress.UnitTest`.
- Create: `Entities/Notification.cs`, `Entities/NotificationList.cs`, `Entities/PushSubscription.cs`, `Entities/NotificationTypes.cs`
- Create: `UseCases/NotificationInteractor.cs`, `UseCases/INotificationDataAccess.cs`, `UseCases/INotificationSeenDataAccess.cs`, `UseCases/IPushSubscriptionDataAccess.cs`, `UseCases/IPushQueue.cs`
- Test: `api/Ez.Hress.UnitTest/Notifications/NotificationInteractorTests.cs`

**Interfaces (produced, used by Tasks 3–7):**
```csharp
namespace Ez.Hress.Notifications.Entities;
public static class NotificationTypes { public const string News = "news"; }
public class Notification {
  public string ID { get; set; } = "";          // set by data access on save
  public string Type { get; set; } = "";
  public string Title { get; set; } = "";
  public string Link { get; set; } = "";
  public int? RecipientUserID { get; set; }      // null = everyone
  public DateTime Created { get; set; }          // UTC
}
public class NotificationList { public DateTime? LastSeen { get; set; } public IList<Notification> Notifications { get; set; } = []; }
public class PushSubscription {
  public string Endpoint { get; set; } = ""; public string P256dh { get; set; } = ""; public string Auth { get; set; } = "";
  public int UserID { get; set; } public string? UserAgent { get; set; } public DateTime Created { get; set; }
}

namespace Ez.Hress.Notifications.UseCases;
public interface INotificationDataAccess {
  Task<Notification> SaveAsync(Notification notification);              // returns it with ID set
  Task<Notification?> GetAsync(string id);
  Task<IList<Notification>> GetLatestAsync(int userID, int top);       // everyone + RecipientUserID == userID, newest first
}
public interface INotificationSeenDataAccess { Task<DateTime?> GetLastSeenAsync(int userID); Task SetLastSeenAsync(int userID, DateTime seen); }
public interface IPushSubscriptionDataAccess {
  Task UpsertAsync(PushSubscription subscription);                      // keyed by endpoint
  Task<PushSubscription?> GetAsync(string endpoint);
  Task DeleteAsync(string endpoint);
  Task<IList<PushSubscription>> GetAllAsync();
}
public interface IPushQueue { Task EnqueueAsync(string notificationID); }

public class NotificationInteractor(INotificationDataAccess notifications, INotificationSeenDataAccess seen,
    IPushSubscriptionDataAccess subscriptions, IPushQueue queue, ILogger<NotificationInteractor> log) {
  public const int TitleMaxLength = 100;
  public Task<Notification> NotifyNewsCreatedAsync(int newsID, string newsTitle);
  public Task<NotificationList> GetNotificationsAsync(int userID, int top = 50);
  public Task<bool> HasUnreadAsync(int userID);
  public Task MarkSeenAsync(int userID);
  public Task SaveSubscriptionAsync(PushSubscription subscription);
  public Task RemoveSubscriptionAsync(int userID, string endpoint);
  public Task<bool> IsSubscribedAsync(int userID, string endpoint);
}
```

- [ ] **Step 1: Write failing tests** in `NotificationInteractorTests` (Moq for all four data interfaces):
  - `NotifyNewsCreated_SavesNewsNotificationAndQueuesIt`: `NotifyNewsCreatedAsync(42, "Bjórkvöld")` saves a notification with `Type == "news"`, `Title == "Bjórkvöld"`, `Link == "/news/42"`, `RecipientUserID == null`, and `Created` within 5s of `UtcNow`. Then it calls `queue.EnqueueAsync(savedID)` once, using the ID returned by `SaveAsync`.
  - `NotifyNewsCreated_TruncatesLongTitle`: a 500×`"a"` title → saved `Title.Length == 100` and ends with `"…"`.
  - `NotifyNewsCreated_InvalidNewsID_Throws` (`[InlineData(0)] [InlineData(-1)]`) → `ArgumentException`, and nothing is saved.
  - `GetNotifications_ReturnsLastSeenBeforeThisVisit`: seen returns `2026-11-01`, data returns 2 items → `LastSeen == 2026-11-01`, `Notifications.Count == 2`, `GetLatestAsync(userID, 50)` called.
  - `HasUnread_NoLastSeen_TrueWhenAnyNotification` / `HasUnread_NoLastSeen_FalseWhenEmpty`.
  - `HasUnread_TrueOnlyWhenNewestIsAfterLastSeen`: newest `Created` equal to `LastSeen` → false, 1 second later → true.
  - `MarkSeen_SetsLastSeenToNow`: `SetLastSeenAsync(userID, t)` with `t` within 5s of `UtcNow`.
  - `SaveSubscription_UpsertsWithCreatedSet`: the subscription is upserted with `Created` set. Upserting the same endpoint for a second user calls `UpsertAsync` with the new `UserID`. Ownership moves because the data access is keyed by endpoint (Review Focus 2).
  - `SaveSubscription_MissingFields_Throws`: empty `Endpoint`, `P256dh` or `Auth` → `ArgumentException`.
  - `RemoveSubscription_OnlyOwner`: if the stored owner is the same user → `DeleteAsync(endpoint)`. If it's another user, or nothing is stored → `DeleteAsync` is never called and nothing throws.
  - `IsSubscribed_TrueOnlyForOwner`: stored for user 5 → true for 5, false for 6, and false when nothing is stored.
- [ ] **Step 2: Run tests to verify they fail.** Run: `cd api && dotnet test --filter "FullyQualifiedName~NotificationInteractorTests"`. Expected: FAIL (types missing).
- [ ] **Step 3: Implement the entities, interfaces and `NotificationInteractor`.** Truncation is `title[..99] + "…"` when it's longer than 100. Log each public method in the house style.
- [ ] **Step 4: Run the tests again.** Expected: PASS.
- [ ] **Step 5: Commit**: `feat(notifications): add notifications domain and interactor`

---

### Task 3: Notifications Table Storage and queue access

**Files:**
- Create in `api/Ez.Hress.Notifications/DataAccess/`: `NotificationTableAccess.cs`, `NotificationTableEntity.cs`, `NotificationSeenTableAccess.cs`, `PushSubscriptionTableAccess.cs`, `PushSubscriptionTableEntity.cs`, `PushQueueStorageAccess.cs`, `NotificationKeys.cs` (internal)
- Test: `api/Ez.Hress.UnitTest/Notifications/NotificationKeysTests.cs`

**Interfaces:**
- Consumes: Task 2's interfaces and entities.
- Produces: `NotificationTableAccess : INotificationDataAccess`, `NotificationSeenTableAccess : INotificationSeenDataAccess`, `PushSubscriptionTableAccess : IPushSubscriptionDataAccess`, `PushQueueStorageAccess : IPushQueue`. Each constructor is `(BlobConnectionInfo connectionInfo, ILogger<T> log)`. `public static class NotificationQueue { public const string Name = "notification-push"; }` in `UseCases/`.
- Keys (my calls, overriding the spec's suggestion for subscriptions so that upsert-by-endpoint works across members):
  - `Notifications`: PartitionKey = `"all"` for everyone, or the recipient's user ID. RowKey = `NotificationKeys.NewestFirst(created)` + `"_"` + a `Guid.NewGuid():N`. The ID is `"{PartitionKey}|{RowKey}"`.
  - `NotificationSeen`: PartitionKey = user ID, RowKey = `"news"`, property `LastSeen`.
  - `PushSubscriptions`: PartitionKey = `"all"`, RowKey = `NotificationKeys.EndpointKey(endpoint)`, properties Endpoint, P256dh, Auth, UserID, UserAgent, Created.
  - Queue message body: the JSON `{"notificationID":"<id>"}`. The `QueueClient` **must** use `new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 }`. The Functions queue trigger expects base64, and plain text ends up in the poison queue. Call `CreateIfNotExists` on the tables and the queue in the constructors, like `BeerSeasonTableAccess`.

```csharp
internal static class NotificationKeys {
  public static string NewestFirst(DateTime createdUtc);   // (DateTime.MaxValue.Ticks - createdUtc.Ticks).ToString("D19")
  public static string EndpointKey(string endpoint);       // lowercase hex SHA-256 of the UTF-8 endpoint
}
```

- [ ] **Step 1: Write failing tests**
  - `NewestFirst_LaterTimeSortsFirst`: `string.CompareOrdinal(NewestFirst(t.AddSeconds(1)), NewestFirst(t)) < 0`, and both are 19 characters long.
  - `EndpointKey_IsTableSafeAndStable` (Review Focus 1): for `"https://fcm.googleapis.com/fcm/send/abc:def?x=1#y"` the key is 64 characters, matches `^[0-9a-f]+$`, and is the same on two calls. A different endpoint gives a different key.
- [ ] **Step 2: Run the tests to verify they fail.** Run: `dotnet test --filter "FullyQualifiedName~NotificationKeysTests"`. Expected: FAIL.
- [ ] **Step 3: Implement `NotificationKeys` and the four data access classes.** `GetLatestAsync` queries the `"all"` partition and the user's partition, merges them, sorts by `Created` descending, and takes `top`. `GetAsync(id)` splits the ID on `|`.
- [ ] **Step 4: Run the tests again, then build.** Expected: PASS, and the build succeeds.
- [ ] **Step 5: Commit**: `feat(notifications): table storage and queue access`

---

### Task 4: Notify when news is created, plus the HTTP endpoints in the existing API

**Files:**
- Modify: `api/Ez.Hress.Scripts/Ez.Hress.Scripts.csproj`: reference `Ez.Hress.Notifications`.
- Modify: `api/Ez.Hress.Scripts/UseCases/NewsInteractor.cs`: the constructor takes `NotificationInteractor`, and `CreateNewsAsync` calls it.
- Modify: `api/Ez.Hress.UnitTest/Scripts/NewsInteractorTests.cs`: every `new NewsInteractor(...)` gets a real `NotificationInteractor` built on mocks.
- Create: `api/Ez.Hress.FunctionsApi/Notifications/NotificationFunctions.cs`
- Modify: `api/Ez.Hress.FunctionsApi/Ez.Hress.FunctionsApi.csproj` (reference the module) and `Program.cs` (DI).

**Interfaces:**
- Consumes: `NotificationInteractor` (Task 2) and the data access classes (Task 3).
- Produces these HTTP routes (all JWT-authenticated except `vapid-public-key`; 401 when the JWT is invalid):

| Function name | Method + route | Request | Response |
|---|---|---|---|
| `notificationsList` | GET `notifications` | – | 200 `{ lastSeen: string \| null, notifications: [{ id, type, title, link, created }] }` |
| `notificationsSeen` | POST `notifications/seen` | – | 204 |
| `notificationsStatus` | GET `notifications/status` | – | 200 `{ hasUnread: bool }` |
| `notificationsVapidKey` | GET `notifications/vapid-public-key` | – | 200 `{ publicKey }` (anonymous) |
| `notificationsSubscribe` | POST `notifications/subscriptions` | `{ endpoint, keys: { p256dh, auth } }` (the browser's `PushSubscription.toJSON()`) | 204, or 400 on missing fields |
| `notificationsUnsubscribe` | DELETE `notifications/subscriptions` | `{ endpoint }` | 204 |
| `notificationsSubscribed` | GET `notifications/subscriptions/current?endpoint=` | – | 200 `{ subscribed: bool }`, or 400 if the endpoint is missing |

Responses use camelCase like the existing functions. `UserAgent` comes from the `User-Agent` request header.

- [ ] **Step 1: Write failing tests** in `NewsInteractorTests`:
  - `CreateNews_NotifiesWithNewsIDAndTitle`: after `CreateNews` returns ID 77, the notification mock's `SaveAsync` receives `Link == "/news/77"` and the news `Name` as `Title`. The queue mock's `EnqueueAsync` is called once.
  - `CreateNews_NotificationFailure_StillReturnsNews`: `SaveAsync` throws `Exception` → `CreateNewsAsync` returns the news with ID 77, and nothing is thrown.
  - `UpdateNews_DoesNotNotify`: `UpdateNewsAsync` never calls `SaveAsync`.
- [ ] **Step 2: Run the tests to verify they fail.** Run: `dotnet test --filter "FullyQualifiedName~NewsInteractorTests"`. Expected: FAIL (compile error from the new constructor parameter).
- [ ] **Step 3: Implement.** In `NewsInteractor`, put a try/catch around `NotifyNewsCreatedAsync(news.ID, news.Name)` that logs an error and continues. In `NotificationFunctions`, follow `NewsFunctions` (auth check, `JsonSerializer` with case-insensitive options, catch `ArgumentException` → 400, log and rethrow everything else). In `Program.cs`, register the four data access classes with `BlobConnectionInfo` and `NotificationInteractor`, and read `Ez.Hress.Notifications.Vapid.PublicKey` from `config` for the key endpoint.
- [ ] **Step 4: Run the full test suite and build the solution.** Expected: PASS.
- [ ] **Step 5: Manual check.** Run `func start` with Azurite and the public key set in `local.settings.json`. `curl localhost:7072/api/notifications/vapid-public-key` returns the key. Creating a news post through the SPA writes a row to `Notifications` and a message to `notification-push`.
- [ ] **Step 6: Commit**: `feat(api): notify on new news and expose notification endpoints`

---

### Task 5: `PushInteractor` and the web push sender

**Files:**
- Create: `api/Ez.Hress.Notifications/Entities/PushPayload.cs`, `Entities/PushRunResult.cs`, `UseCases/IPushSender.cs`, `UseCases/PushInteractor.cs`, `DataAccess/WebPushSender.cs`, `DataAccess/VapidSettings.cs`
- Modify: the module csproj: add `Lib.Net.Http.WebPush` (latest stable).
- Test: `api/Ez.Hress.UnitTest/Notifications/PushInteractorTests.cs`

**Interfaces:**
```csharp
public record PushPayload(string Title, string Body, string Link);          // serialized camelCase: { title, body, link }
public enum PushSendResult { Sent, Gone, Failed }
public record PushRunResult(int Sent, int Removed, int Failed);
public interface IPushSender { Task<PushSendResult> SendAsync(PushSubscription subscription, PushPayload payload); }
public class PushInteractor(INotificationDataAccess notifications, IPushSubscriptionDataAccess subscriptions,
    IPushSender sender, ILogger<PushInteractor> log) {
  public const string NewsPushTitle = "Ný frétt";
  public Task<PushRunResult> SendAsync(string notificationID);
}
public class VapidSettings(string publicKey, string privateKey, string subject);
public class WebPushSender(VapidSettings vapid, HttpClient httpClient, ILogger<WebPushSender> log) : IPushSender;
```
`WebPushSender` maps HTTP 404/410 from the push service to `Gone`, any other failure to `Failed`, and success to `Sent`. Use the library's own subscription type with a `using` alias to avoid clashing with our `PushSubscription`. TTL is 24 hours.

- [ ] **Step 1: Write failing tests**
  - `Send_NewsNotification_SendsTitleBodyLinkToAll`: 3 subscriptions → 3 sends with `PushPayload("Ný frétt", notification.Title, notification.Link)`, and the result is `(3, 0, 0)`.
  - `Send_GoneSubscriptionIsDeleted_OthersStillSent` (Review Focus 3): the results are Sent, Gone, Failed, Sent → `DeleteAsync` is called only for the Gone endpoint, and the result is `(2, 1, 1)`.
  - `Send_SenderThrows_CountsAsFailedAndContinues`: one `SendAsync` throws `HttpRequestException` → the others are still sent.
  - `Send_RecipientSet_OnlyRecipientsDevices`: `RecipientUserID = 5` → only subscriptions with `UserID == 5` are sent to.
  - `Send_NotificationMissing_ReturnsZeroWithoutThrowing`: `GetAsync` returns null → `(0, 0, 0)`, and the sender is never called.
  - `Send_StorageFailure_Throws`: `GetAllAsync` throws → the exception propagates, so the queue retries.
- [ ] **Step 2: Run the tests to verify they fail.** Run: `dotnet test --filter "FullyQualifiedName~PushInteractorTests"`. Expected: FAIL.
- [ ] **Step 3: Implement `PushInteractor`, then `WebPushSender`** (VAPID auth via `VapidAuthentication(publicKey, privateKey) { Subject = subject }`).
- [ ] **Step 4: Run the tests again.** Expected: PASS.
- [ ] **Step 5: Commit**: `feat(notifications): send web push to subscribed devices`

---

### Task 6: New Function App `Ez.Hress.NotificationFunctions`, its deploy workflow and the owner's setup guide

**Files:**
- Create: `api/Ez.Hress.NotificationFunctions/Ez.Hress.NotificationFunctions.csproj`: copy the shape of `Ez.Hress.FunctionsApi.csproj` (net10.0, v4, Exe), keeping only `Microsoft.Azure.Functions.Worker`, `.Sdk`, `Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues`, the App Insights packages, and a reference to `Ez.Hress.Notifications` and `Ez.Hress.Shared`. Add it to the solution.
- Create: `Program.cs`, `NotificationPushFunction.cs`, `host.json` (copy from the API), `local.settings.example.json` (keys only, no values), `README.md` (the owner's setup checklist below).
- Create: `.github/workflows/notification-functions.yml`

**Interfaces:**
- Consumes: `PushInteractor`, `WebPushSender`, `VapidSettings`, `PushSubscriptionTableAccess`, `NotificationTableAccess`, `NotificationQueue.Name`.
- Produces: `[Function("notificationPush")] Task Run([QueueTrigger(NotificationQueue.Name, Connection = "ContentStorage")] NotificationPushMessage message)`, where `public record NotificationPushMessage(string NotificationID)`. It calls `PushInteractor.SendAsync` and logs the `PushRunResult`.
- `Program.cs` reads `ContentStorage`, `Ez.Hress.Notifications.Vapid.PublicKey`, `...PrivateKey` and `...Subject` from environment variables only. It doesn't read the SQL configuration source, and it fails fast on startup with a clear message if any of them is missing. It registers `AddHttpClient<IPushSender, WebPushSender>()`.

- [ ] **Step 1: Create the project and function.** Build: `cd api && dotnet build Ez.Hress.FunctionsApi.sln`. Expected: the build succeeds.
- [ ] **Step 2: Manual check with Azurite.** Run this app locally (`func start --port 7073`). Enqueue a message for an unknown ID through `PushQueueStorageAccess.EnqueueAsync("all|missing")`, for example from a scratch test or by creating news via the API from Task 4. The function logs `PushRunResult(0, 0, 0)` and `notification-push-poison` stays empty. That proves the base64 encoding from Task 3 and the `{ notificationID }` message shape match the trigger. The full push end-to-end check is in Task 10, Step 5.
- [ ] **Step 3: Write `.github/workflows/notification-functions.yml`.** It triggers on push to `main` for paths `api/**`, sets up .NET 10, runs `dotnet test api/Ez.Hress.FunctionsApi.sln`, publishes `api/Ez.Hress.NotificationFunctions` (Release), and deploys with `Azure/functions-action@v1` using the secrets `NOTIFICATION_FUNCTIONS_APP_NAME` and `NOTIFICATION_FUNCTIONS_PUBLISH_PROFILE`.
- [ ] **Step 4: Write `README.md`** with the owner's checklist from the spec's "Azure setup" section, spelled out:
  1. `npx web-push generate-vapid-keys`. Keep the private key out of the repo.
  2. Create the Function App: Linux, consumption, .NET 10 isolated, with its own storage account for `AzureWebJobsStorage`.
  3. Function App settings: `ContentStorage` (the content storage connection string), `Ez.Hress.Notifications.Vapid.PublicKey`, `...PrivateKey`, `...Subject` (`mailto:` contact).
  4. Static Web App environment variables: `Ez.Hress.Notifications.Vapid.PublicKey`.
  5. GitHub secrets: `NOTIFICATION_FUNCTIONS_APP_NAME`, `NOTIFICATION_FUNCTIONS_PUBLISH_PROFILE`.
  6. The queue and tables are created by the code on first use.
- [ ] **Step 5: Commit**: `feat(notifications): add push Function App and deploy workflow`

---

### Task 7: Service worker, manifest icons and caching

**Files:**
- Create: `public/sw.js`, `public/logo192.png`, `public/logo512.png`, `src/pushServiceWorker.ts`
- Modify: `src/index.js`: replace the `serviceWorker` import and `serviceWorker.unregister()` with `registerPushServiceWorker()`.
- Delete: `src/serviceWorker.js`
- Modify: `public/manifest.json` (icons `logo192.png` 192x192 and `logo512.png` 512x512, both `image/png`; keep `favicon.ico`), `public/index.html` (`<link rel="apple-touch-icon" href="%PUBLIC_URL%/logo192.png">`), `staticwebapp.config.json` (a route `/sw.js` with `"cache-control": "no-cache"`).
- Test: `src/pushServiceWorker.test.ts`

**Interfaces:**
- Produces: `export function registerPushServiceWorker(): Promise<ServiceWorkerRegistration | undefined>`. It registers `/sw.js`, or does nothing when `"serviceWorker" in navigator` is false, and logs registration errors to the console without throwing.
- `sw.js` handles `push` by parsing `{ title, body, link }` from `event.data.json()` and calling `showNotification(title, { body, icon: "/logo192.png", data: { link } })`. It handles `notificationclick` by closing the notification, then focusing a same-origin window client and navigating it to `link`, or `clients.openWindow(link)` if there's none. There's no `fetch` handler and no caching.

- [ ] **Step 1: Write failing tests**: `registers /sw.js when supported` (`navigator.serviceWorker.register` is called with `"/sw.js"`), `does nothing when unsupported`, and `swallows registration errors`.
- [ ] **Step 2: Run the tests to verify they fail.** Run: `npm test -- --watchAll=false --testPathPattern=pushServiceWorker`. Expected: FAIL.
- [ ] **Step 3: Implement it.** Make the icons with Pillow (it's installed): `python3 -c "from PIL import Image; im=Image.open('public/logo.png'); [im.resize((s,s), Image.LANCZOS).save(f'public/logo{s}.png') for s in (192,512)]"`.
- [ ] **Step 4: Run the tests, then `npm run dbuild`.** Expected: PASS, and the build succeeds with `build/sw.js` present.
- [ ] **Step 5: Commit**: `feat(web): push service worker and installable icons`

---

### Task 8: Browser push helpers and notification hooks

**Files:**
- Create: `src/pages/notifications/push.ts`, `src/pages/notifications/hooks/useNotifications.ts`, `src/pages/notifications/hooks/useNotificationStatus.ts`, `src/pages/notifications/types.ts`
- Test: `src/pages/notifications/push.test.ts`

**Interfaces:**
```ts
// types.ts
export type NotificationItem = { id: string; type: string; title: string; link: string; created: string };
export type NotificationListResponse = { lastSeen: string | null; notifications: NotificationItem[] };
export type PushState = "unsupported" | "ios-install" | "blocked" | "off" | "on";

// push.ts — token is the raw JWT (authTokens.token)
export function isIos(): boolean;                 // iPhone/iPod/iPad UA, or platform "MacIntel" with maxTouchPoints > 1
export function isStandalone(): boolean;          // matchMedia("(display-mode: standalone)") or navigator.standalone
export async function getPushState(token: string): Promise<PushState>;
export async function enablePush(token: string): Promise<void>;   // requestPermission → key → subscribe → POST
export async function disablePush(token: string): Promise<void>;  // DELETE (errors ignored) → subscription.unsubscribe()

// hooks — query keys are ["notifications"] and ["notifications", "status"]
export const useNotifications: () => UseQueryResult<NotificationListResponse>;
export const useMarkNotificationsSeen: () => UseMutationResult<void, unknown, void>;  // onSuccess: setQueryData(["notifications","status"], { hasUnread: false })
export const useNotificationStatus: () => UseQueryResult<{ hasUnread: boolean }>;     // enabled only when logged in; retry false
```
The order in `getPushState`: iOS and not standalone → `ios-install` (checked first, because iOS Safari outside the home screen has no `PushManager`). Then no `serviceWorker`, `PushManager` or `Notification` → `unsupported`. Then `Notification.permission === "denied"` → `blocked`. Then the browser has no subscription → `off`. Then `GET subscriptions/current` says `subscribed` → `on`, otherwise `off`. The VAPID key is converted with a `urlBase64ToUint8Array` helper in the same file.

- [ ] **Step 1: Write failing tests** in `push.test.ts` (mock `axios`, `react-global-configuration`, `navigator.serviceWorker`, `window.PushManager` and `Notification`):
  - `isIos` is true for an iPhone UA, true for `MacIntel` + `maxTouchPoints 5` (iPad), and false for desktop Chrome.
  - `getPushState` returns `ios-install` on iPhone when not standalone, `unsupported` without `PushManager`, `blocked` when permission is denied, `off` without a browser subscription, `on` when the server says `{ subscribed: true }`, and `off` when the browser has a subscription but the server says `{ subscribed: false }`.
  - `enablePush` POSTs to `/api/notifications/subscriptions` with `subscription.toJSON()` and the auth header, and when permission isn't granted it rejects without subscribing.
  - `disablePush` still calls `unsubscribe()` when the DELETE rejects.
- [ ] **Step 2: Run the tests to verify they fail.** Run: `npm test -- --watchAll=false --testPathPattern=notifications/push`. Expected: FAIL.
- [ ] **Step 3: Implement it**, following `src/hooks/useCreateNews.ts` for Axios and the auth header.
- [ ] **Step 4: Run the tests again.** Expected: PASS.
- [ ] **Step 5: Commit**: `feat(web): push helpers and notification hooks`

---

### Task 9: Notifications page with the push banner

**Files:**
- Create: `src/pages/notifications/index.tsx` (`NotificationsPage`), `src/pages/notifications/components/PushBanner.tsx`, `src/pages/notifications/notifications.css`
- Modify: `src/pages/App.js`: `<Route path="notifications" element={<RequireLogin><NotificationsPage /></RequireLogin>} />`
- Test: `src/pages/notifications/index.test.tsx`, `src/pages/notifications/components/PushBanner.test.tsx`

**Interfaces:**
- Consumes: Task 8 hooks and `push.ts`.
- The page renders the `PushBanner`, then the list. Each item shows the title, "Ný frétt" for type `news`, and a relative time, and links to `item.link` via React Router `Link`. An item gets class `unread` when `lastSeen === null || created > lastSeen`. After the list loads successfully, the page calls `useMarkNotificationsSeen` once.
- When the list is empty, it says so (suggested wording: "Engar tilkynningar ennþá.").
- `PushBanner` shows one block per `PushState` (suggested Icelandic wording; exact copy is left to the builder):
  - `unsupported`: nothing.
  - `ios-install`: how to add Hress.Org to the home screen (Share → "Add to Home Screen") to get notifications.
  - `blocked`: notifications are blocked in the browser settings for hress.org and must be allowed there.
  - `off`: "Fáðu tilkynningu í þetta tæki þegar ný frétt birtist." with a "Kveikja" button.
  - `on`: a single line, "Kveikt á tilkynningum í þessu tæki", with a "Slökkva" button.
- If enable or disable fails, it shows a short error and goes back to the state from `getPushState`.

- [ ] **Step 1: Write failing tests**
  - `index.test.tsx`: 3 items with `lastSeen` between them → 2 have class `unread`. `lastSeen: null` → all are unread. POST `/api/notifications/seen` is called exactly once after the list loads. An empty list shows the empty text. Clicking an item navigates to `/news/42`.
  - `PushBanner.test.tsx` (mock `./push`): each state shows its text and button. Clicking "Kveikja" calls `enablePush(token)` and then shows the `on` line. If `enablePush` rejects, an error is shown and the "Kveikja" button is back. Clicking "Slökkva" calls `disablePush`.
- [ ] **Step 2: Run the tests to verify they fail.** Run: `npm test -- --watchAll=false --testPathPattern=pages/notifications`. Expected: FAIL.
- [ ] **Step 3: Implement it.** Styling matches the existing post/sidebar look. Unread items get the left border accent from the mockup.
- [ ] **Step 4: Run the tests again.** Expected: PASS.
- [ ] **Step 5: Commit**: `feat(web): notifications page with push banner`

---

### Task 10: Header bell, and turning push off at sign-out

**Files:**
- Create: `src/pages/frame/notificationBell.tsx`, `src/pages/frame/notificationBell.css`
- Modify: `src/pages/App.js`: render `<NotificationBell />` as the first `li` in `nav.main ul`, before Search.
- Modify: `src/pages/frame/menu.js`: `logout` turns push off before clearing tokens.
- Test: `src/pages/frame/notificationBell.test.tsx`, `src/pages/frame/menu.test.js` (new)

**Interfaces:**
- Consumes: `useNotificationStatus`, `disablePush` (Task 8).
- `NotificationBell` renders nothing when `authTokens` is missing. Otherwise it renders `<li className="notifications"><Link className="fa-bell" to="/notifications">Tilkynningar</Link>{hasUnread && <span className="notification-dot" />}</li>`. The existing header CSS hides the text and draws the icon. It refetches the status on route change (`useLocation().pathname` in the query key or a `refetch` effect). If the status query errors (for example a 401), it shows no dot and doesn't crash.
- `logout` in `menu.js`: `await Promise.race([disablePush(authTokens.token).catch(() => {}), timeout(3000)])`, then the existing `setAuthTokens(); navigate("/");`.

- [ ] **Step 1: Write failing tests**
  - Bell: renders nothing when logged out. When logged in with `{ hasUnread: true }` it shows the link to `/notifications` and the dot. With `{ hasUnread: false }` there's no dot. If the status request rejects with 401, there's a link and no dot, and nothing throws.
  - Menu (Review Focus 5): clicking "Útskráning" calls `disablePush` with the token, and then `setAuthTokens` is called with no arguments. If `disablePush` rejects, the user is still logged out. If `disablePush` never resolves, the user is logged out after 3000 ms (jest fake timers).
- [ ] **Step 2: Run the tests to verify they fail.** Run: `npm test -- --watchAll=false --testPathPattern="frame/(notificationBell|menu)"`. Expected: FAIL.
- [ ] **Step 3: Implement it.** The dot sits at the top right of the bell (absolute, teal `#2ebaae`, like the mockup).
- [ ] **Step 4: Run the full suite and the build.** Run: `npm test -- --watchAll=false && npm run dbuild`. Expected: PASS, and the build succeeds.
- [ ] **Step 5: Manual end-to-end check** (`npm start`, the API on 7072 and the push app on 7073, all on Azurite):
  - Logged out, there's no bell.
  - Logged in, turn on push from `/notifications`.
  - Create a news post. A browser notification "Ný frétt / <title>" appears, and clicking it opens `/news/{id}`.
  - The bell shows a dot after navigation. Opening `/notifications` lists the post highlighted, and the dot disappears.
  - Sign out, and the subscription row is gone from `PushSubscriptions`.
- [ ] **Step 6: Commit**: `feat(web): header notification bell and sign-out push cleanup`

---

## Spec coverage check

| Spec requirement | Task |
|---|---|
| .NET 10 upgrade + SWA runtime + CLAUDE.md | 1 |
| Notification / subscription / seen model, LastSeen semantics, recipient field | 2, 3 |
| News create → notification + queue, failure isolated, edit doesn't notify | 4 |
| HTTP endpoints including `subscriptions/current` | 4 |
| Push sender, prune 404/410, retry via queue on storage failure, "Ný frétt" + title | 5 |
| Separate Function App, deploy, owner checklist, VAPID keys | 6 |
| Service worker, click opens post, manifest icons (upscaled), apple-touch-icon, no-cache sw.js, delete CRA SW | 7 |
| Banner states (unsupported / iOS / blocked / off / on), server-confirmed "on" | 8, 9 |
| `/notifications` page, highlight unread-on-arrival, opening clears dot | 9 |
| Bell for logged-in only, dot, sign-out turns push off | 10 |
