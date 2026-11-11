import axios from "axios";
import { disablePush, enablePush, getPushState, isIos } from "./push";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const mockedAxios = axios as jest.Mocked<typeof axios>;
const base = "http://localhost:7072/api/notifications";
const headers = { "X-Custom-Authorization": "token jwt" };

const IPHONE_UA =
  "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15";
const CHROME_UA =
  "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 Chrome/120.0 Safari/537.36";

function setNavigator(props: Record<string, unknown>) {
  for (const [key, value] of Object.entries(props)) {
    Object.defineProperty(window.navigator, key, {
      value,
      configurable: true,
    });
  }
}

function setStandalone(standalone: boolean) {
  window.matchMedia = jest.fn().mockReturnValue({ matches: standalone });
}

function setPermission(permission: string, requestResult = permission) {
  Object.defineProperty(window, "Notification", {
    value: {
      permission,
      requestPermission: jest.fn().mockResolvedValue(requestResult),
    },
    configurable: true,
    writable: true,
  });
}

const subscription = {
  endpoint: "https://push.example/abc def",
  toJSON: () => ({
    endpoint: "https://push.example/abc def",
    keys: { p256dh: "p", auth: "a" },
  }),
  unsubscribe: jest.fn().mockResolvedValue(true),
};

const pushManager = {
  getSubscription: jest.fn(),
  subscribe: jest.fn(),
};

beforeEach(() => {
  jest.resetAllMocks();
  subscription.unsubscribe.mockResolvedValue(true);
  setNavigator({
    userAgent: CHROME_UA,
    platform: "Linux x86_64",
    maxTouchPoints: 0,
    standalone: undefined,
    serviceWorker: {
      ready: Promise.resolve({ pushManager }),
    },
  });
  setStandalone(false);
  Object.defineProperty(window, "PushManager", {
    value: function PushManager() {},
    configurable: true,
    writable: true,
  });
  setPermission("default");
});

describe("isIos", () => {
  it("is true for an iPhone user agent", () => {
    setNavigator({ userAgent: IPHONE_UA });
    expect(isIos()).toBe(true);
  });

  it("is true for an iPad reporting MacIntel with touch points", () => {
    setNavigator({ platform: "MacIntel", maxTouchPoints: 5 });
    expect(isIos()).toBe(true);
  });

  it("is false for desktop Chrome", () => {
    expect(isIos()).toBe(false);
  });
});

describe("getPushState", () => {
  it("returns ios-install on iPhone when not standalone", async () => {
    setNavigator({ userAgent: IPHONE_UA });
    expect(await getPushState("jwt")).toBe("ios-install");
  });

  it("returns unsupported without PushManager", async () => {
    delete (window as any).PushManager;
    expect(await getPushState("jwt")).toBe("unsupported");
  });

  it("returns blocked when permission is denied", async () => {
    setPermission("denied");
    expect(await getPushState("jwt")).toBe("blocked");
  });

  it("returns off without a browser subscription", async () => {
    pushManager.getSubscription.mockResolvedValue(null);
    expect(await getPushState("jwt")).toBe("off");
    expect(mockedAxios.get).not.toHaveBeenCalled();
  });

  it("returns on when the server says subscribed", async () => {
    pushManager.getSubscription.mockResolvedValue(subscription);
    mockedAxios.get.mockResolvedValue({ data: { subscribed: true } });
    expect(await getPushState("jwt")).toBe("on");
    expect(mockedAxios.get).toHaveBeenCalledWith(
      `${base}/subscriptions/current?endpoint=${encodeURIComponent(subscription.endpoint)}`,
      { headers }
    );
  });

  it("returns off when the server says subscribed false", async () => {
    pushManager.getSubscription.mockResolvedValue(subscription);
    mockedAxios.get.mockResolvedValue({ data: { subscribed: false } });
    expect(await getPushState("jwt")).toBe("off");
  });

  it("returns off when the server omits subscribed (null/default omitted)", async () => {
    pushManager.getSubscription.mockResolvedValue(subscription);
    mockedAxios.get.mockResolvedValue({ data: {} });
    expect(await getPushState("jwt")).toBe("off");
  });
});

describe("enablePush", () => {
  it("posts the subscription with the auth header", async () => {
    setPermission("default", "granted");
    mockedAxios.get.mockResolvedValue({ data: { publicKey: "AQAB" } });
    pushManager.subscribe.mockResolvedValue(subscription);
    mockedAxios.post.mockResolvedValue({ status: 204 });

    await enablePush("jwt");

    expect(pushManager.subscribe).toHaveBeenCalledWith(
      expect.objectContaining({ userVisibleOnly: true })
    );
    expect(mockedAxios.post).toHaveBeenCalledWith(
      `${base}/subscriptions`,
      subscription.toJSON(),
      { headers }
    );
  });

  it("rejects without subscribing when permission is not granted", async () => {
    setPermission("default", "denied");

    await expect(enablePush("jwt")).rejects.toThrow();
    expect(pushManager.subscribe).not.toHaveBeenCalled();
    expect(mockedAxios.post).not.toHaveBeenCalled();
  });
});

describe("disablePush", () => {
  it("still unsubscribes when the DELETE rejects", async () => {
    pushManager.getSubscription.mockResolvedValue(subscription);
    mockedAxios.delete.mockRejectedValue(new Error("network"));
    jest.spyOn(console, "error").mockImplementation(() => {});

    await disablePush("jwt");

    expect(mockedAxios.delete).toHaveBeenCalledWith(`${base}/subscriptions`, {
      data: { endpoint: subscription.endpoint },
      headers,
    });
    expect(subscription.unsubscribe).toHaveBeenCalled();
  });
});
