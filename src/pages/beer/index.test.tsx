import { render, screen, waitFor } from "@testing-library/react";
import axios from "axios";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import BeerHome from "./index";
import { AuthContext } from "../../context/auth";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

describe("BeerHome", () => {
  let queryClient: QueryClient;

  const renderComponent = () =>
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <AuthContext.Provider value={{ authTokens: { token: "fake-token" } }}>
            <BeerHome />
          </AuthContext.Provider>
        </MemoryRouter>
      </QueryClientProvider>
    );

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
  });

  it("shows the current season's name", async () => {
    (axios.get as jest.Mock).mockResolvedValue({
      data: { id: "jolabjor-2026", name: "Jólabjór 2026", isOpen: true },
    });

    renderComponent();

    expect(await screen.findByText("Jólabjór 2026")).toBeInTheDocument();
    expect(screen.queryByTestId("season-closed")).not.toBeInTheDocument();
  });

  it("shows an empty state when there is no season", async () => {
    (axios.get as jest.Mock).mockRejectedValue({ response: { status: 404 } });

    renderComponent();

    expect(await screen.findByTestId("no-season")).toBeInTheDocument();
  });

  it("marks a closed season as finished", async () => {
    (axios.get as jest.Mock).mockResolvedValue({
      data: {
        id: "jolabjor-2025",
        name: "Jólabjór 2025",
        isOpen: false,
        endDate: "2026-01-06T00:00:00Z",
      },
    });

    renderComponent();

    expect(await screen.findByTestId("season-closed")).toBeInTheDocument();
  });

  it("treats a season without an isOpen key as closed", async () => {
    // The backend omits default values, so a closed season has no isOpen.
    (axios.get as jest.Mock).mockResolvedValue({
      data: { id: "jolabjor-2025", name: "Jólabjór 2025" },
    });

    renderComponent();

    expect(await screen.findByTestId("season-closed")).toBeInTheDocument();
  });

  it("treats an open season without an endDate as open", async () => {
    (axios.get as jest.Mock).mockResolvedValue({
      data: { id: "jolabjor-2026", name: "Jólabjór 2026", isOpen: true },
    });

    renderComponent();

    await screen.findByText("Jólabjór 2026");
    expect(screen.queryByTestId("season-closed")).not.toBeInTheDocument();
  });

  it("sends the Hress token header", async () => {
    (axios.get as jest.Mock).mockResolvedValue({
      data: { id: "jolabjor-2026", name: "Jólabjór 2026", isOpen: true },
    });

    renderComponent();

    await waitFor(() => {
      expect(axios.get).toHaveBeenCalledWith(
        "http://localhost:7072/api/beer/seasons/current",
        { headers: { "X-Custom-Authorization": "token fake-token" } }
      );
    });
  });
});
