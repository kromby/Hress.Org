import { registerPushServiceWorker } from "./pushServiceWorker";

describe("registerPushServiceWorker", () => {
  const original = Object.getOwnPropertyDescriptor(navigator, "serviceWorker");

  afterEach(() => {
    if (original) {
      Object.defineProperty(navigator, "serviceWorker", original);
    } else {
      delete (navigator as Partial<Navigator>).serviceWorker;
    }
    jest.restoreAllMocks();
  });

  it("registers /sw.js when supported", async () => {
    const registration = {} as ServiceWorkerRegistration;
    const register = jest.fn().mockResolvedValue(registration);
    Object.defineProperty(navigator, "serviceWorker", {
      value: { register },
      configurable: true,
    });

    await expect(registerPushServiceWorker()).resolves.toBe(registration);
    expect(register).toHaveBeenCalledWith("/sw.js");
  });

  it("does nothing when unsupported", async () => {
    delete (navigator as Partial<Navigator>).serviceWorker;

    await expect(registerPushServiceWorker()).resolves.toBeUndefined();
  });

  it("swallows registration errors", async () => {
    const error = new Error("boom");
    const register = jest.fn().mockRejectedValue(error);
    Object.defineProperty(navigator, "serviceWorker", {
      value: { register },
      configurable: true,
    });
    const consoleError = jest
      .spyOn(console, "error")
      .mockImplementation(() => undefined);

    await expect(registerPushServiceWorker()).resolves.toBeUndefined();
    expect(consoleError).toHaveBeenCalled();
  });
});
