# Ez.Hress.NotificationFunctions

Azure Function app that sends web push notifications. It has two functions:

- `notificationPush` (queue trigger): the Static Web App API, or `hardheadReminder`, enqueues
  `{"notificationID":"<id>"}` on queue `notification-push`; this loads the notification and pushes it
  to the subscribed devices (everyone's, or only the recipient's).
- `hardheadReminder` (timer, 09:00 UTC daily, which is 09:00 in Iceland all year): if a Hardhead night is
  dated today, everyone who hosted or attended a night in the previous 6 months gets a notification
  ("Harðhaus #N er í kvöld hjá …"). A night still on its placeholder date (last day of the month, never
  edited) is skipped. Running it again the same day sends nothing new.

## Owner setup checklist

1. Generate VAPID keys: `npx web-push generate-vapid-keys`. Keep the private key out of the repo.
2. Create the Function App: **Flex Consumption** plan (Linux), .NET 10 isolated, with its own
   storage account for `AzureWebJobsStorage`. The production app is `ez-func-hressorg-push` in
   `ez-rg-hressorg`. Then turn on **SCM Basic Auth Publishing Credentials**
   (Function App -> Settings -> Configuration -> General settings); it is off by default for new apps, and without it
   the downloaded publish profile has no usable credentials and the first deploy fails with 401.
   The deploy workflow sets `sku: flexconsumption`; without it a publish-profile deploy falls back to Kudu
   zipdeploy, which Flex doesn't have (405 on app settings, then 404 on zipdeploy). If the app is ever moved
   to a Windows Consumption plan, remove that `sku` line.
3. Function App settings:
   - `ContentStorage`: the content storage account connection string (the one holding the notification tables and the queue).
   - `Ez.Hress.Notifications.Vapid.PublicKey`
   - `Ez.Hress.Notifications.Vapid.PrivateKey`
   - `Ez.Hress.Notifications.Vapid.Subject`: a `mailto:` contact, for example `mailto:you@example.com`.

   The app refuses to start if any of the four is missing.
   - `Ez.Hress.Database.ConnectionString`: the Hress SQL database (same value as in the Static Web App).
     Used only by `hardheadReminder`, which logs a warning and skips its run while it's missing. The
     Azure SQL server's firewall must let the Function App in ("Allow Azure services and resources to
     access this server", or the app's outbound IPs).
4. Static Web App environment variables: `Ez.Hress.Notifications.Vapid.PublicKey` (same public key).
5. GitHub repository secrets: `NOTIFICATION_FUNCTIONS_APP_NAME` and `NOTIFICATION_FUNCTIONS_PUBLISH_PROFILE`
   (download the publish profile from the Function App, after enabling SCM Basic Auth Publishing Credentials as in step 2).

Finish this whole checklist before merging: the deploy workflow runs on the first `api/**` push to main.
6. The queue and tables are created by the code on first use; nothing to provision.

## Local run

Copy `local.settings.example.json` to `local.settings.json`, fill in the values (Azurite is fine:
`UseDevelopmentStorage=true`), then `func start --port 7073`. Never commit `local.settings.json`.
