export async function registerPushServiceWorker(): Promise<
  ServiceWorkerRegistration | undefined
> {
  if (!("serviceWorker" in navigator)) {
    return undefined;
  }

  try {
    return await navigator.serviceWorker.register("/sw.js");
  } catch (error) {
    console.error("Service worker registration failed", error);
    return undefined;
  }
}
