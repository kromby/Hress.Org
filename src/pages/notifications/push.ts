import axios from "axios";
import config from "react-global-configuration";
import { PushState } from "./types";

const baseUrl = () => `${config.get("apiPath")}/api/notifications`;
const authHeaders = (token: string) => ({
  "X-Custom-Authorization": `token ${token}`,
});

export function isIos(): boolean {
  const ua = navigator.userAgent;
  if (/iPhone|iPod|iPad/.test(ua)) {
    return true;
  }
  return navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1;
}

export function isStandalone(): boolean {
  const mql =
    typeof window.matchMedia === "function"
      ? window.matchMedia("(display-mode: standalone)")
      : undefined;
  return (
    Boolean(mql?.matches) ||
    (navigator as Navigator & { standalone?: boolean }).standalone === true
  );
}

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
  const raw = window.atob(base64);
  const output = new Uint8Array(raw.length);
  for (let i = 0; i < raw.length; i++) {
    output[i] = raw.charCodeAt(i);
  }
  return output;
}

// Used when subscribing: waits for the service worker to become active.
function getReadyRegistration(): Promise<ServiceWorkerRegistration> {
  return navigator.serviceWorker.ready;
}

// Returns the current browser push subscription, or null. Never throws or
// hangs: unsupported browsers, no registered service worker and lookup
// failures all resolve to null.
async function getCurrentSubscription(): Promise<PushSubscription | null> {
  if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
    return null;
  }
  try {
    const registration = await navigator.serviceWorker.getRegistration();
    return (await registration?.pushManager.getSubscription()) ?? null;
  } catch (error) {
    console.error("Failed to look up push subscription", error);
    return null;
  }
}

export async function getPushState(token: string): Promise<PushState> {
  if (isIos() && !isStandalone()) {
    return "ios-install";
  }
  if (
    !("serviceWorker" in navigator) ||
    !("PushManager" in window) ||
    !("Notification" in window)
  ) {
    return "unsupported";
  }
  if (Notification.permission === "denied") {
    return "blocked";
  }

  const subscription = await getCurrentSubscription();
  if (!subscription) {
    return "off";
  }

  try {
    const response = await axios.get<{ subscribed?: boolean }>(
      `${baseUrl()}/subscriptions/current?endpoint=${encodeURIComponent(subscription.endpoint)}`,
      { headers: authHeaders(token) }
    );
    return response.data?.subscribed === true ? "on" : "off";
  } catch (error) {
    console.error("Failed to check push subscription on server", error);
    return "off";
  }
}

export async function enablePush(token: string): Promise<void> {
  const permission = await Notification.requestPermission();
  if (permission !== "granted") {
    throw new Error("Notification permission was not granted");
  }

  const registration = await getReadyRegistration();
  const keyResponse = await axios.get<{ publicKey: string }>(
    `${baseUrl()}/vapid-public-key`
  );
  const subscription = await registration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: urlBase64ToUint8Array(
      keyResponse.data.publicKey
    ) as BufferSource,
  });

  await axios.post(`${baseUrl()}/subscriptions`, subscription.toJSON(), {
    headers: authHeaders(token),
  });
}

export async function disablePush(token: string): Promise<void> {
  const subscription = await getCurrentSubscription();
  if (!subscription) {
    return;
  }

  try {
    await axios.delete(`${baseUrl()}/subscriptions`, {
      data: { endpoint: subscription.endpoint },
      headers: authHeaders(token),
    });
  } catch (error) {
    console.error("Failed to delete push subscription on server", error);
  }
  try {
    await subscription.unsubscribe();
  } catch (error) {
    console.error("Failed to unsubscribe in browser", error);
  }
}
