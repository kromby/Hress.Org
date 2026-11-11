import { act, fireEvent, render, screen } from "@testing-library/react";
import axios from "axios";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext } from "../../context/auth";
import Menu from "./menu";
import { disablePush } from "../notifications/push";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));
jest.mock("../notifications/push", () => ({ disablePush: jest.fn() }));

const setAuthTokens = jest.fn();
let client = null;

const renderMenu = async () => {
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <AuthContext.Provider
          value={{ authTokens: { token: "tok" }, setAuthTokens }}
        >
          <Menu visible onClick={jest.fn()} />
        </AuthContext.Provider>
      </MemoryRouter>
    </QueryClientProvider>
  );
  // Let the menu's own data load settle before the test acts.
  await act(() => Promise.resolve());
};

const clickLogout = async () => {
  await act(async () => {
    fireEvent.click(await screen.findByText("Útskráning"));
  });
};

describe("Menu logout", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    axios.get.mockResolvedValue({ data: [] });
    client = new QueryClient();
  });
  afterEach(() => jest.useRealTimers());

  it("turns push off with the token, then clears the tokens", async () => {
    disablePush.mockResolvedValue();
    await renderMenu();
    await clickLogout();
    expect(disablePush).toHaveBeenCalledWith("tok");
    expect(setAuthTokens).toHaveBeenCalledTimes(1);
    expect(setAuthTokens).toHaveBeenCalledWith();
  });

  it("ignores a second click while sign-out is in progress", async () => {
    disablePush.mockReturnValue(new Promise(() => undefined));
    await renderMenu();
    await screen.findByText("Útskráning");
    jest.useFakeTimers();
    fireEvent.click(screen.getByText("Útskráning"));
    fireEvent.click(screen.getByText("Útskráning"));
    expect(disablePush).toHaveBeenCalledTimes(1);
    await act(async () => {
      jest.advanceTimersByTime(3000);
      await Promise.resolve();
    });
    expect(setAuthTokens).toHaveBeenCalledTimes(1);
  });

  it("still logs out when disablePush rejects", async () => {
    disablePush.mockRejectedValue(new Error("boom"));
    await renderMenu();
    await clickLogout();
    expect(setAuthTokens).toHaveBeenCalledWith();
  });

  it("logs out after 3000 ms when disablePush never resolves", async () => {
    disablePush.mockReturnValue(new Promise(() => undefined));
    await renderMenu();
    await screen.findByText("Útskráning");
    jest.useFakeTimers();
    fireEvent.click(screen.getByText("Útskráning"));
    expect(setAuthTokens).not.toHaveBeenCalled();
    await act(async () => {
      jest.advanceTimersByTime(2999);
      await Promise.resolve();
    });
    expect(setAuthTokens).not.toHaveBeenCalled();
    await act(async () => {
      jest.advanceTimersByTime(1);
      await Promise.resolve();
    });
    expect(setAuthTokens).toHaveBeenCalledWith();
  });
});
