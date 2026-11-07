import { fireEvent, render, screen } from "@testing-library/react";
import axios from "axios";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import RegisterBeerPage from "./register";
import { AuthContext } from "../../context/auth";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const openSeason = { id: "jolabjor-2026", name: "Jólabjór 2026", isOpen: true };
const closedSeason = { id: "jolabjor-2025", name: "Jólabjór 2025" };

const matches = [
  {
    beer: {
      id: "k1",
      name: "Jóla Kaldi",
      producer: "Bruggsmiðjan Kaldi",
      alcoholPercentage: 5.4,
      country: "Ísland",
    },
    latestSeasonID: "jolabjor-2025",
    latestSeasonName: "Jólabjór 2025",
  },
  {
    beer: {
      id: "e1",
      name: "Jóla Egils",
      producer: "Ölgerðin",
      alcoholPercentage: 5.6,
      country: "Ísland",
    },
    latestSeasonID: "jolabjor-2026",
    latestSeasonName: "Jólabjór 2026",
    inCurrentSeason: true,
  },
];

describe("RegisterBeerPage", () => {
  let queryClient: QueryClient;

  const mockGet = (season: object = openSeason) =>
    (axios.get as jest.Mock).mockImplementation((url: string) =>
      Promise.resolve({
        data: url.includes("/beer/beers?") ? matches : season,
      })
    );

  const renderPage = () =>
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={["/beer/register"]}>
          <AuthContext.Provider
            value={{
              authTokens: { token: "fake-token" },
              setAuthTokens: jest.fn(),
            }}
          >
            <Routes>
              <Route path="/beer/register" element={<RegisterBeerPage />} />
              <Route
                path="/beer/:beerID"
                element={<p data-testid="beer-page">beer page</p>}
              />
            </Routes>
          </AuthContext.Provider>
        </MemoryRouter>
      </QueryClientProvider>
    );

  const typeName = async (text = "jola") => {
    fireEvent.change(await screen.findByLabelText("Nafn"), {
      target: { value: text },
    });
    return screen.findByText("Jóla Kaldi");
  };

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    mockGet();
  });

  it("shows the page for the current open season", async () => {
    renderPage();

    expect(
      await screen.findByRole("heading", { name: "Skrá nýjan bjór" })
    ).toBeInTheDocument();
    expect(await screen.findByText("Jólabjór 2026")).toBeInTheDocument();
  });

  it("says beers can't be added when the season is closed", async () => {
    mockGet(closedSeason);
    renderPage();

    expect(
      await screen.findByText("Tímabilið er búið og ekki hægt að skrá bjór.")
    ).toBeInTheDocument();
    expect(screen.queryByLabelText("Nafn")).not.toBeInTheDocument();
  });

  it("lists matching beers as I type the name", async () => {
    renderPage();
    await typeName();

    const items = screen.getAllByRole("listitem");
    expect(items).toHaveLength(2);
    expect(items[0]).toHaveTextContent("Bruggsmiðjan Kaldi · 5,4%");
    expect(items[0]).toHaveTextContent("Bæta við");
    expect(items[1]).toHaveTextContent("er á tímabilinu");
    expect(items[1]).not.toHaveTextContent("Bæta við");
    expect(screen.getByRole("link", { name: /Jóla Egils/ })).toHaveAttribute(
      "href",
      "/beer/e1"
    );
    expect(
      screen.getByText("Finnurðu hann ekki? Haltu áfram að skrá hann.")
    ).toBeInTheDocument();
  });

  it("Bæta við adds the beer and opens its page", async () => {
    (axios.post as jest.Mock).mockResolvedValue({
      data: matches[0].beer,
    });
    renderPage();
    await typeName();

    fireEvent.click(screen.getByRole("button", { name: /Bæta við/ }));

    expect(await screen.findByTestId("beer-page")).toBeInTheDocument();
    expect(axios.post).toHaveBeenCalledWith(
      expect.stringContaining("/seasons/jolabjor-2026/beers"),
      { beerID: "k1" },
      expect.anything()
    );
  });

  it("disables Bæta við while adding", async () => {
    (axios.post as jest.Mock).mockImplementation(() => new Promise(() => {}));
    renderPage();
    await typeName();
    const button = screen.getByRole("button", { name: /Bæta við/ });

    fireEvent.click(button);
    await screen.findByRole("button", { name: /Bæta við/ });
    expect(button).toBeDisabled();
    fireEvent.click(button);

    expect(axios.post).toHaveBeenCalledTimes(1);
  });

  it("shows an error and stays when adding fails", async () => {
    (axios.post as jest.Mock).mockRejectedValue({ response: { status: 500 } });
    renderPage();
    await typeName();

    fireEvent.click(screen.getByRole("button", { name: /Bæta við/ }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Ekki tókst að bæta bjórnum við. Reyndu aftur."
    );
    expect(screen.queryByTestId("beer-page")).not.toBeInTheDocument();
    expect(screen.getByLabelText("Nafn")).toBeInTheDocument();
  });
});
