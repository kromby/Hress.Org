import { ReactNode } from "react";
import { renderHook, waitFor } from "@testing-library/react";
import axios from "axios";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext } from "../../../context/auth";
import { useNotifications } from "./useNotifications";
import { useNotificationStatus } from "./useNotificationStatus";

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
          { authTokens: { token: "jwt" }, setAuthTokens: jest.fn() } as any
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
