import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import axios from "axios";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import BeerPage from "./beer";
import { AuthContext } from "../../context/auth";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const openSeason = { id: "jolabjor-2026", name: "Jólabjór 2026", isOpen: true };
const closedSeason = { id: "jolabjor-2025", name: "Jólabjór 2025" };

const summaries = [
  {
    beer: {
      id: "b1",
      name: "Tuborg Lille Jul",
      producer: "Carlsberg",
      alcoholPercentage: 5.4,
      country: "Danmörk",
      vinbudinUrl: "https://www.vinbudin.is/b1",
    },
  },
  {
    beer: {
      id: "b2",
      name: "Egils Jólabjór",
      producer: "Ölgerðin",
      alcoholPercentage: 5.6,
      country: "Ísland",
    },
    myRating: 2,
  },
];

const chatMessage = {
  id: 1,
  userID: 8,
  username: "gummi",
  text: "Sætur en góður.",
  created: "2026-11-05T18:30:00Z",
};

const ratingUrl =
  "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers/b1/rating";

describe("BeerPage", () => {
  let queryClient: QueryClient;
  const setAuthTokens = jest.fn();

  const mockGet = (season: object = openSeason, messages: object[] = []) =>
    (axios.get as jest.Mock).mockImplementation((url: string) =>
      Promise.resolve({
        data: url.endsWith("/beers")
          ? summaries
          : url.endsWith("/messages")
            ? messages
            : season,
      })
    );

  const renderPage = (beerID = "b1") =>
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={[`/beer/${beerID}`]}>
          <AuthContext.Provider
            value={{ authTokens: { token: "fake-token" }, setAuthTokens }}
          >
            <Routes>
              <Route path="/beer/:beerID" element={<BeerPage />} />
            </Routes>
          </AuthContext.Provider>
        </MemoryRouter>
      </QueryClientProvider>
    );

  const stars = () =>
    screen.getByTestId("rating-stars").querySelectorAll("svg");

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    mockGet();
  });

  it("shows name, producer, country, alcohol % and a Vínbúðin link", async () => {
    renderPage();

    expect(await screen.findByText("Tuborg Lille Jul")).toBeInTheDocument();
    const details = screen.getByTestId("beer-details");
    expect(details).toHaveTextContent("Carlsberg");
    expect(details).toHaveTextContent("Danmörk");
    expect(details).toHaveTextContent("5,4%");
    expect(screen.getByRole("link", { name: /Vínbúðinni/ })).toHaveAttribute(
      "href",
      "https://www.vinbudin.is/b1"
    );
  });

  it("shows five stars and a hint when I haven't rated the beer", async () => {
    renderPage();

    await screen.findByTestId("rating-stars");
    expect(stars()).toHaveLength(5);
    expect(screen.getByTestId("rating-hint")).toBeInTheDocument();
  });

  it("shows my existing rating", async () => {
    renderPage("b2");

    expect(await screen.findByTestId("rating-stars")).toHaveAttribute(
      "aria-label",
      "Þín einkunn: 2 af 5"
    );
  });

  it("tells me I can change my existing rating", async () => {
    renderPage("b2");

    expect(await screen.findByTestId("rating-hint")).toHaveTextContent(
      "Smelltu á aðra stjörnu til að breyta einkunninni."
    );
  });

  it("changing my rating saves the new stars and shows them", async () => {
    (axios.put as jest.Mock).mockResolvedValue({ data: { stars: 5 } });
    renderPage("b2");
    await screen.findByTestId("rating-stars");

    fireEvent.click(stars()[4]);

    await waitFor(() =>
      expect(axios.put).toHaveBeenCalledWith(
        expect.stringContaining("/beers/b2/rating"),
        { stars: 5 },
        expect.anything()
      )
    );
    expect(await screen.findByTestId("rating-saved")).toHaveTextContent(
      "Einkunn vistuð: 5 af 5"
    );
    expect(screen.getByTestId("rating-stars")).toHaveAttribute(
      "aria-label",
      "Þín einkunn: 5 af 5"
    );
  });

  it("a tap while saving is sent after the earlier save, so the last tap counts", async () => {
    const resolves: ((value: unknown) => void)[] = [];
    (axios.put as jest.Mock).mockImplementation(
      () => new Promise((resolve) => resolves.push(resolve))
    );
    renderPage();
    await screen.findByTestId("rating-stars");

    fireEvent.click(stars()[2]);
    await waitFor(() => expect(axios.put).toHaveBeenCalledTimes(1));
    fireEvent.click(stars()[4]);

    // The stars follow the last tap, but the second save waits.
    await waitFor(() =>
      expect(screen.getByTestId("rating-stars")).toHaveAttribute(
        "aria-label",
        "Þín einkunn: 5 af 5"
      )
    );
    expect(axios.put).toHaveBeenCalledTimes(1);

    resolves[0]({ data: { stars: 3 } });
    await waitFor(() => expect(axios.put).toHaveBeenCalledTimes(2));
    expect((axios.put as jest.Mock).mock.calls.map(([, body]) => body)).toEqual(
      [{ stars: 3 }, { stars: 5 }]
    );
    expect(screen.getByTestId("rating-saving")).toBeInTheDocument();

    resolves[1]({ data: { stars: 5 } });
    expect(await screen.findByTestId("rating-saved")).toHaveTextContent(
      "Einkunn vistuð: 5 af 5"
    );
  });

  it("tapping the 4th star saves {stars: 4} and confirms", async () => {
    (axios.put as jest.Mock).mockResolvedValue({
      data: { seasonID: "jolabjor-2026", beerID: "b1", userID: 7, stars: 4 },
    });
    renderPage();
    await screen.findByTestId("rating-stars");

    fireEvent.click(stars()[3]);

    await waitFor(() =>
      expect(axios.put).toHaveBeenCalledWith(
        ratingUrl,
        { stars: 4 },
        { headers: { "X-Custom-Authorization": "token fake-token" } }
      )
    );
    expect(await screen.findByTestId("rating-saved")).toHaveTextContent(
      "Einkunn vistuð: 4 af 5"
    );
    expect(screen.getByTestId("rating-stars")).toHaveAttribute(
      "aria-label",
      "Þín einkunn: 4 af 5"
    );
  });

  it("exposes the stars as a radio group I can rate with the keyboard", async () => {
    (axios.put as jest.Mock).mockResolvedValue({ data: { stars: 3 } });
    renderPage("b2");
    await screen.findByRole("radiogroup");

    const radios = screen.getAllByRole("radio");
    expect(radios).toHaveLength(5);
    // Only my current rating is in the tab order.
    expect(radios[1]).toHaveAttribute("aria-checked", "true");
    expect(radios[1]).toHaveAttribute("tabindex", "0");
    expect(radios[0]).toHaveAttribute("tabindex", "-1");

    radios[1].focus();
    fireEvent.keyDown(radios[1], { key: "ArrowRight" });
    expect(radios[2]).toHaveFocus();
    expect(axios.put).not.toHaveBeenCalled();

    fireEvent.keyDown(radios[2], { key: "Enter" });

    await waitFor(() =>
      expect(axios.put).toHaveBeenCalledWith(
        expect.stringContaining("/beers/b2/rating"),
        { stars: 3 },
        expect.anything()
      )
    );
  });

  it("refetches the season's beers after saving", async () => {
    (axios.put as jest.Mock).mockResolvedValue({ data: { stars: 4 } });
    renderPage();
    await screen.findByTestId("rating-stars");
    const beerFetches = () =>
      (axios.get as jest.Mock).mock.calls.filter(([url]) =>
        url.endsWith("/beers")
      ).length;
    const before = beerFetches();

    fireEvent.click(stars()[3]);

    await waitFor(() => expect(beerFetches()).toBeGreaterThan(before));
  });

  it("shows an error when saving fails", async () => {
    (axios.put as jest.Mock).mockRejectedValue({ response: { status: 500 } });
    renderPage();
    await screen.findByTestId("rating-stars");

    fireEvent.click(stars()[0]);

    expect(await screen.findByTestId("rating-error")).toBeInTheDocument();
  });

  it("stars are read-only when the season is closed", async () => {
    mockGet(closedSeason);
    renderPage();
    await screen.findByTestId("rating-stars");

    fireEvent.click(stars()[3]);

    expect(axios.put).not.toHaveBeenCalled();
    expect(screen.getByTestId("rating-closed")).toBeInTheDocument();
    expect(screen.getByTestId("rating-stars")).toHaveAttribute("role", "img");
    expect(screen.queryAllByRole("radio")).toHaveLength(0);
  });

  it("shows the beer's chat before I've rated it", async () => {
    mockGet(openSeason, [chatMessage]);
    renderPage("b1");

    expect(await screen.findByText("Sætur en góður.")).toBeInTheDocument();
    expect(screen.getByTestId("rating-hint")).toBeInTheDocument();
    expect(axios.get).toHaveBeenCalledWith(
      "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers/b1/messages",
      expect.anything()
    );
  });

  it("shows the chat but no message box when the season is closed", async () => {
    mockGet(closedSeason, [chatMessage]);
    renderPage();

    expect(await screen.findByText("Sætur en góður.")).toBeInTheDocument();
    expect(screen.queryByTestId("chat-composer")).toBeNull();
    expect(screen.getByTestId("chat-closed")).toBeInTheDocument();
  });

  it("has a message box when the season is open", async () => {
    renderPage();

    expect(await screen.findByTestId("chat-composer")).toBeInTheDocument();
  });

  it("shows not-found for a beer that isn't in the season", async () => {
    renderPage("nope");

    expect(await screen.findByTestId("beer-not-found")).toBeInTheDocument();
  });
});
