# Ez.Hress.NotificationFunctions

Queue-triggered Azure Function app that sends web push notifications. The Static Web App API
enqueues `{"notificationID":"<id>"}` on queue `notification-push`; this app loads the
notification and pushes it to the subscribed devices.

## Owner setup checklist

1. Generate VAPID keys: `npx web-push generate-vapid-keys`. Keep the private key out of the repo.
2. Create the Function App: **Windows** Consumption plan, .NET 10 isolated, Functions v4, with its own
   storage account for `AzureWebJobsStorage`. (Flex Consumption is the alternative, but it needs the workflow to log in
   with `azure/login` (OIDC) instead of a publish profile.)
3. Function App settings:
   - `ContentStorage`: the content storage account connection string (the one holding the notification tables and the queue).
   - `Ez.Hress.Notifications.Vapid.PublicKey`
   - `Ez.Hress.Notifications.Vapid.PrivateKey`
   - `Ez.Hress.Notifications.Vapid.Subject`: a `mailto:` contact, for example `mailto:you@example.com`.

   The app refuses to start if any of the four is missing.
4. Static Web App environment variables: `Ez.Hress.Notifications.Vapid.PublicKey` (same public key).
5. GitHub repository secrets: `NOTIFICATION_FUNCTIONS_APP_NAME` and `NOTIFICATION_FUNCTIONS_PUBLISH_PROFILE`
   (download the publish profile from the Function App).
6. The queue and tables are created by the code on first use; nothing to provision.

## Local run

Copy `local.settings.example.json` to `local.settings.json`, fill in the values (Azurite is fine:
`UseDevelopmentStorage=true`), then `func start --port 7073`. Never commit `local.settings.json`.
