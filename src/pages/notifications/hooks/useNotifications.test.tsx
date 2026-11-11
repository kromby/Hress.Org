import { ReactNode } from "react";
import { act, renderHook, waitFor } from "@testing-library/react";
import axios from "axios";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext } from "../../../context/auth";
import { useMarkNotificationsSeen, useNotifications } from "./useNotifications";
import {
  notificationStatusKey,
  useNotificationStatus,
} from "./useNotificationStatus";
import { clearNotificationCache } from "../clearNotificationCache";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const mockedAxios = axios as jest.Mocked<typeof axios>;

function wrapper({ children }: { children: ReactNode }) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return (
    <QueryClientProvider client={client}>
      <AuthContext.Provider
        value={
          { authTokens: { token: "jwt" }, setAuthTokens: jest.fn() } as never
        }
      >
        {children}
      </AuthContext.Provider>
    </QueryClientProvider>
  );
}

describe("notification hooks normalization", () => {
  it("status {} becomes hasUnread false", async () => {
    mockedAxios.get.mockResolvedValue({ data: {} });
    const { result } = renderHook(() => useNotificationStatus(), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual({ hasUnread: false });
  });

  it("list without lastSeen or notifications is normalized", async () => {
    mockedAxios.get.mockResolvedValue({ data: {} });
    const { result } = renderHook(() => useNotifications(), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual({ lastSeen: null, notifications: [] });
  });

  it("list keeps provided values", async () => {
    const item = {
      id: "1",
      type: "news",
      title: "t",
      link: "/n/1",
      created: "c",
    };
    mockedAxios.get.mockResolvedValue({
      data: { lastSeen: "2026-01-01", notifications: [item] },
    });
    const { result } = renderHook(() => useNotifications(), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual({
      lastSeen: "2026-01-01",
      notifications: [item],
    });
  });
});

describe("useMarkNotificationsSeen", () => {
  type Auth = {
    authTokens: { token: string } | null;
    setAuthTokens: jest.Mock;
  };
  const setup = (initial: Auth) => {
    const client = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    const state = { auth: initial };
    const localWrapper = ({ children }: { children: ReactNode }) => (
      <QueryClientProvider client={client}>
        <AuthContext.Provider value={state.auth as never}>
          {children}
        </AuthContext.Provider>
      </QueryClientProvider>
    );
    return { client, state, localWrapper };
  };
  const signedIn = (token: string): Auth => ({
    authTokens: { token },
    setAuthTokens: jest.fn(),
  });
  const pendingPost = () => {
    const control = { resolve: jest.fn() as (v: unknown) => void };
    mockedAxios.post.mockReturnValue(
      new Promise((resolve) => {
        control.resolve = resolve;
      })
    );
    return control;
  };

  it("drops an in-flight stale status and refetches after marking seen", async () => {
    const { client, localWrapper } = setup(signedIn("jwt"));
    const stale = { resolve: jest.fn() as (v: unknown) => void };
    mockedAxios.get
      .mockReturnValueOnce(
        new Promise((resolve) => {
          stale.resolve = resolve;
        })
      )
      .mockResolvedValue({ data: {} });
    mockedAxios.post.mockResolvedValue({});
    const status = renderHook(() => useNotificationStatus(), {
      wrapper: localWrapper,
    });
    const seen = renderHook(() => useMarkNotificationsSeen(), {
      wrapper: localWrapper,
    });
    await act(async () => {
      await seen.result.current.mutateAsync({});
    });
    await act(async () => {
      stale.resolve({ data: { hasUnread: true } });
      await Promise.resolve();
    });
    expect(mockedAxios.get).toHaveBeenCalledTimes(2);
    expect(status.result.current.data).toEqual({ hasUnread: false });
    expect(client.getQueryData(notificationStatusKey("jwt"))).toEqual({
      hasUnread: false,
    });
  });

  it("keeps the dot when the refetch reports a newer unread notification", async () => {
    const { localWrapper } = setup(signedIn("jwt"));
    mockedAxios.get
      .mockResolvedValueOnce({ data: { hasUnread: true } })
      .mockResolvedValueOnce({ data: { hasUnread: true } });
    mockedAxios.post.mockResolvedValue({});
    const status = renderHook(() => useNotificationStatus(), {
      wrapper: localWrapper,
    });
    await waitFor(() =>
      expect(status.result.current.data).toEqual({ hasUnread: true })
    );
    const seen = renderHook(() => useMarkNotificationsSeen(), {
      wrapper: localWrapper,
    });
    await act(async () => {
      await seen.result.current.mutateAsync({ upTo: "2026-03-01T00:00:00Z" });
    });
    await waitFor(() => expect(mockedAxios.get).toHaveBeenCalledTimes(2));
    await waitFor(() =>
      expect(status.result.current.data).toEqual({ hasUnread: true })
    );
  });

  it("sends upTo in the body when given", async () => {
    mockedAxios.post.mockResolvedValue({});
    const seen = renderHook(() => useMarkNotificationsSeen(), { wrapper });
    await act(async () => {
      await seen.result.current.mutateAsync({ upTo: "2026-03-01T00:00:00Z" });
    });
    expect(mockedAxios.post).toHaveBeenCalledWith(
      "http://localhost:7072/api/notifications/seen",
      { upTo: "2026-03-01T00:00:00Z" },
      { headers: { "X-Custom-Authorization": "token jwt" } }
    );
  });

  it("leaves the next member's status alone when the token changed before it resolved", async () => {
    const { client, state, localWrapper } = setup(signedIn("tokA"));
    const post = pendingPost();
    client.setQueryData(notificationStatusKey("tokA"), { hasUnread: true });
    const seen = renderHook(() => useMarkNotificationsSeen(), {
      wrapper: localWrapper,
    });
    act(() => {
      seen.result.current.mutate({});
    });
    clearNotificationCache(client);
    state.auth = signedIn("tokB");
    client.setQueryData(notificationStatusKey("tokB"), { hasUnread: true });
    seen.rerender();
    await act(async () => {
      post.resolve({});
      await Promise.resolve();
    });
    await waitFor(() => expect(seen.result.current.isSuccess).toBe(true));
    expect(client.getQueryData(notificationStatusKey("tokB"))).toEqual({
      hasUnread: true,
    });
    expect(client.getQueryData(notificationStatusKey("tokA"))).toBeUndefined();
  });

  it("leaves the next member's status alone when the page unmounted before it resolved", async () => {
    const { client, state, localWrapper } = setup(signedIn("tokA"));
    const post = pendingPost();
    client.setQueryData(notificationStatusKey("tokA"), { hasUnread: true });
    const seen = renderHook(() => useMarkNotificationsSeen(), {
      wrapper: localWrapper,
    });
    act(() => {
      seen.result.current.mutate({});
    });
    // Member A signs out: the page unmounts and the cache is cleared.
    seen.unmount();
    clearNotificationCache(client);
    state.auth = signedIn("tokB");
    client.setQueryData(notificationStatusKey("tokB"), { hasUnread: true });
    await act(async () => {
      post.resolve({});
      await new Promise((resolve) => setTimeout(resolve, 0));
    });
    await waitFor(() =>
      expect(client.getMutationCache().getAll()[0]?.state.status).toBe(
        "success"
      )
    );
    expect(client.getQueryData(notificationStatusKey("tokB"))).toEqual({
      hasUnread: true,
    });
    expect(client.getQueryData(notificationStatusKey("tokA"))).toBeUndefined();
  });
});
