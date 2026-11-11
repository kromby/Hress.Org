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
    !!mql?.matches ||
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

async function getRegistration(): Promise<ServiceWorkerRegistration> {
  return navigator.serviceWorker.ready;
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

  const registration = await getRegistration();
  const subscription = await registration.pushManager.getSubscription();
  if (!subscription) {
    return "off";
  }

  const response = await axios.get<{ subscribed?: boolean }>(
    `${baseUrl()}/subscriptions/current?endpoint=${encodeURIComponent(subscription.endpoint)}`,
    { headers: authHeaders(token) }
  );
  return response.data?.subscribed === true ? "on" : "off";
}

export async function enablePush(token: string): Promise<void> {
  const permission = await Notification.requestPermission();
  if (permission !== "granted") {
    throw new Error("Notification permission was not granted");
  }

  const registration = await getRegistration();
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
  const registration = await getRegistration();
  const subscription = await registration.pushManager.getSubscription();
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
  await subscription.unsubscribe();
}
