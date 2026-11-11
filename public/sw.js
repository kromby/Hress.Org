// Push-only service worker: no fetch handler, no caching.
self.addEventListener("push", (event) => {
  let payload = {};
  try {
    payload = event.data ? event.data.json() : {};
  } catch (e) {
    payload = {};
  }
  if (!payload || typeof payload !== "object") {
    payload = {};
  }

  const title = payload.title || "Ný frétt";
  const link = typeof payload.link === "string" && payload.link ? payload.link : "/";

  event.waitUntil(
    self.registration.showNotification(title, {
      body: payload.body || "",
      icon: "/logo192.png",
      data: { link },
    })
  );
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();
  const link = (event.notification.data && event.notification.data.link) || "/";
  const target = new URL(link, self.location.origin);
  if (target.origin !== self.location.origin) {
    return;
  }

  event.waitUntil(
    self.clients
      .matchAll({ type: "window", includeUncontrolled: true })
      .then((clients) => {
        const client = clients.find((c) => c.url.startsWith(self.location.origin));
        if (client) {
          return client.focus().then((focused) => (focused || client).navigate(target.href));
        }
        return self.clients.openWindow(target.href);
      })
  );
});
