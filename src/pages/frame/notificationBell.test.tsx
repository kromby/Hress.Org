import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import axios from "axios";
import { Link, MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext } from "../../context/auth";
import NotificationBell from "./notificationBell";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const mockedAxios = axios as jest.Mocked<typeof axios>;

const renderBell = (loggedIn: boolean) =>
  render(
    <QueryClientProvider client={new QueryClient()}>
      <MemoryRouter>
        <AuthContext.Provider
          value={
            {
              authTokens: loggedIn ? { token: "tok" } : undefined,
              setAuthTokens: jest.fn(),
            } as never
          }
        >
          <ul>
            <NotificationBell />
          </ul>
          <Link to="/other">go</Link>
        </AuthContext.Provider>
      </MemoryRouter>
    </QueryClientProvider>
  );

describe("NotificationBell", () => {
  beforeEach(() => jest.clearAllMocks());

  it("renders nothing when logged out", () => {
    renderBell(false);
    expect(screen.queryByRole("link", { name: "Tilkynningar" })).toBeNull();
    expect(mockedAxios.get).not.toHaveBeenCalled();
  });

  it("shows the link and dot when there are unread notifications", async () => {
    mockedAxios.get.mockResolvedValue({ data: { hasUnread: true } });
    const { container } = renderBell(true);
    const link = screen.getByRole("link", { name: "Tilkynningar" });
    expect(link.getAttribute("href")).toBe("/notifications");
    await waitFor(() =>
      expect(container.querySelector(".notification-dot")).not.toBeNull()
    );
  });

  it("shows no dot when nothing is unread", async () => {
    mockedAxios.get.mockResolvedValue({ data: {} });
    const { container } = renderBell(true);
    await waitFor(() => expect(mockedAxios.get).toHaveBeenCalled());
    expect(screen.getByRole("link", { name: "Tilkynningar" })).toBeTruthy();
    expect(container.querySelector(".notification-dot")).toBeNull();
  });

  it("shows the link and no dot when the status request fails", async () => {
    mockedAxios.get.mockRejectedValue({ response: { status: 401 } });
    const { container } = renderBell(true);
    await waitFor(() => expect(mockedAxios.get).toHaveBeenCalled());
    expect(screen.getByRole("link", { name: "Tilkynningar" })).toBeTruthy();
    expect(container.querySelector(".notification-dot")).toBeNull();
  });

  it("fetches the status again after navigating to another route", async () => {
    mockedAxios.get.mockResolvedValue({ data: { hasUnread: false } });
    renderBell(true);
    await waitFor(() => expect(mockedAxios.get).toHaveBeenCalledTimes(1));
    fireEvent.click(screen.getByText("go"));
    await waitFor(() => expect(mockedAxios.get).toHaveBeenCalledTimes(2));
  });
});
