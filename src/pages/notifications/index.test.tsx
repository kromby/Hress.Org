import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import axios from "axios";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext } from "../../context/auth";
import NotificationsPage from "./index";
import { getPushState } from "./push";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));
jest.mock("./push", () => ({
  getPushState: jest.fn(),
  enablePush: jest.fn(),
  disablePush: jest.fn(),
}));

const mockedAxios = axios as jest.Mocked<typeof axios>;

const item = (id: string, created: string, link = `/news/${id}`) => ({
  id,
  type: "news",
  title: `Frétt ${id}`,
  link,
  created,
});

const renderPage = (
  client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
) => {
  return render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={["/notifications"]}>
        <AuthContext.Provider
          value={
            { authTokens: { token: "tok" }, setAuthTokens: jest.fn() } as never
          }
        >
          <Routes>
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/news/:id" element={<div>Frétt opnuð</div>} />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    </QueryClientProvider>
  );
};

const mockList = (data: object) => {
  mockedAxios.get.mockResolvedValue({ data });
  mockedAxios.post.mockResolvedValue({});
};

describe("NotificationsPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (getPushState as jest.Mock).mockResolvedValue("unsupported");
  });

  it("marks only items newer than lastSeen as unread", async () => {
    mockList({
      lastSeen: "2026-03-02T00:00:00Z",
      notifications: [
        item("3", "2026-03-03T00:00:00Z"),
        item("2", "2026-03-02T12:00:00Z"),
        item("1", "2026-03-01T00:00:00Z"),
      ],
    });
    renderPage();
    const items = await screen.findAllByRole("listitem");
    expect(items).toHaveLength(3);
    expect(items.filter((i) => i.classList.contains("unread"))).toHaveLength(2);
    expect(items[2]).not.toHaveClass("unread");
  });

  it("marks all as unread when lastSeen is null", async () => {
    mockList({
      lastSeen: null,
      notifications: [
        item("2", "2026-03-02T00:00:00Z"),
        item("1", "2026-03-01T00:00:00Z"),
      ],
    });
    renderPage();
    const items = await screen.findAllByRole("listitem");
    expect(items.every((i) => i.classList.contains("unread"))).toBe(true);
  });

  it("posts /seen exactly once after the list loads", async () => {
    mockList({
      lastSeen: null,
      notifications: [item("1", "2026-03-01T00:00:00Z")],
    });
    renderPage();
    await screen.findByText("Frétt 1");
    await waitFor(() => expect(mockedAxios.post).toHaveBeenCalledTimes(1));
    expect(mockedAxios.post.mock.calls[0][0]).toBe(
      "http://localhost:7072/api/notifications/seen"
    );
    // The highlight is not recomputed after the seen mutation.
    expect(screen.getByRole("listitem")).toHaveClass("unread");
  });

  it("shows the empty text", async () => {
    mockList({});
    renderPage();
    expect(
      await screen.findByText("Engar tilkynningar ennþá.")
    ).toBeInTheDocument();
  });

  it("navigates to the news link when clicked", async () => {
    mockList({
      lastSeen: null,
      notifications: [item("42", "2026-03-01T00:00:00Z")],
    });
    renderPage();
    fireEvent.click(await screen.findByRole("link", { name: /Frétt 42/ }));
    expect(await screen.findByText("Frétt opnuð")).toBeInTheDocument();
  });

  it("ignores a cached list: posts /seen and highlights only after the fresh load", async () => {
    const client = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    client.setQueryData(["notifications"], {
      lastSeen: "2026-01-01T00:00:00Z",
      notifications: [item("old", "2026-01-02T00:00:00Z")],
    });
    let resolveGet: (v: unknown) => void = jest.fn();
    mockedAxios.get.mockReturnValue(
      new Promise((resolve) => {
        resolveGet = resolve;
      }) as Promise<never>
    );
    mockedAxios.post.mockResolvedValue({});
    renderPage(client);

    await waitFor(() => expect(mockedAxios.get).toHaveBeenCalled());
    expect(screen.queryByText("Frétt old")).toBeNull();
    expect(mockedAxios.post).not.toHaveBeenCalled();

    resolveGet({
      data: {
        lastSeen: "2026-03-02T00:00:00Z",
        notifications: [
          item("new", "2026-03-03T00:00:00Z"),
          item("read", "2026-03-01T00:00:00Z"),
        ],
      },
    });
    const items = await screen.findAllByRole("listitem");
    expect(items[0]).toHaveClass("unread");
    expect(items[1]).not.toHaveClass("unread");
    await waitFor(() => expect(mockedAxios.post).toHaveBeenCalledTimes(1));
  });
});
