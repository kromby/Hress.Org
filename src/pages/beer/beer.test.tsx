import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
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
      imageID: 279076,
      style: "Lager",
      description: "Danskur jólabjór.\nLéttur og sætur.",
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

  it("shows the beer's description with its line breaks, under a heading", async () => {
    renderPage();

    expect(
      await screen.findByRole("heading", { name: "Bjórinn" })
    ).toBeInTheDocument();
    const description = screen.getByTestId("beer-description");
    expect(description.textContent).toBe("Danskur jólabjór.\nLéttur og sætur.");
    expect(description).toHaveStyle({ whiteSpace: "pre-wrap" });
  });

  it("lists the style, brewery, country and strength as labelled facts", async () => {
    renderPage();

    const details = await screen.findByTestId("beer-details");
    const cells = within(details)
      .getAllByText(/./, { selector: "strong" })
      .map((label) => label.parentElement?.textContent);
    expect(cells).toEqual([
      "TegundLager",
      "BrugghúsCarlsberg",
      "LandDanmörk",
      "Styrkleiki5,4%",
    ]);
  });

  it("leaves out the description and style when the beer has none", async () => {
    renderPage("b2");

    const details = await screen.findByTestId("beer-details");
    expect(screen.queryByTestId("beer-description")).not.toBeInTheDocument();
    expect(details).not.toHaveTextContent("Tegund");
    expect(details).toHaveTextContent("Brugghús");
  });

  it("shows the beer's cover photo next to its name", async () => {
    renderPage();

    expect(
      await screen.findByRole("img", { name: "Tuborg Lille Jul" })
    ).toHaveAttribute(
      "src",
      "http://localhost:7072/api/images/279076/content?width=192"
    );
    expect(
      screen.queryByTestId("beer-cover-placeholder")
    ).not.toBeInTheDocument();
  });

  it("shows a placeholder for a beer without a cover photo", async () => {
    renderPage("b2");

    expect(
      await screen.findByTestId("beer-cover-placeholder")
    ).toHaveTextContent("MYND");
    expect(
      screen.queryByRole("img", { name: "Egils Jólabjór" })
    ).not.toBeInTheDocument();
  });

  it("shows five stars and no instructions when I haven't rated the beer", async () => {
    renderPage();

    await screen.findByTestId("rating-stars");
    expect(stars()).toHaveLength(5);
    expect(screen.queryByText(/Smelltu/)).not.toBeInTheDocument();
  });

  it("shows my existing rating", async () => {
    renderPage("b2");

    expect(await screen.findByTestId("rating-stars")).toHaveAttribute(
      "aria-label",
      "Þín einkunn: 2 af 5"
    );
  });

  it("puts the rating in the post's footer, like the Hardhead page", async () => {
    renderPage();

    const rating = await screen.findByTestId("rating-stars");
    expect(rating.closest("article.post > footer ul.stats")).not.toBeNull();
    expect(screen.getByTestId("beer-details").closest("footer")).toBeNull();
    // An empty actions list before the stats pushes them to the right.
    const footer = rating.closest("footer");
    expect(footer?.firstElementChild).toHaveClass("actions");
    expect(footer?.firstElementChild).toBeEmptyDOMElement();
  });

  it("names my existing rating", async () => {
    renderPage("b2");

    expect(await screen.findByTestId("rating-label")).toHaveTextContent(
      "Slakur bjór"
    );
  });

  it("names each number of stars while hovering, from Vondur to Frábær bjór", async () => {
    renderPage();
    await screen.findByTestId("rating-stars");
    const label = screen.getByTestId("rating-label");
    expect(label).toHaveTextContent(/^\s*$/);

    const expected = [
      "Vondur bjór",
      "Slakur bjór",
      "Ágætur bjór",
      "Góður bjór",
      "Frábær bjór",
    ];
    stars().forEach((star, index) => {
      fireEvent.mouseEnter(star);
      expect(label).toHaveTextContent(expected[index]);
    });

    fireEvent.mouseLeave(stars()[4]);
    expect(label).toHaveTextContent(/^\s*$/);
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

  it("names each star radio and previews its label on keyboard focus", async () => {
    renderPage("b2");
    await screen.findByRole("radiogroup");

    expect(
      screen.getByRole("radio", { name: "1 af 5: Vondur bjór" })
    ).toBeInTheDocument();
    const great = screen.getByRole("radio", { name: "5 af 5: Frábær bjór" });
    const label = screen.getByTestId("rating-label");
    expect(label).toHaveTextContent("Slakur bjór");

    fireEvent.focus(great);
    expect(label).toHaveTextContent("Frábær bjór");

    fireEvent.blur(great);
    expect(label).toHaveTextContent("Slakur bjór");
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
    expect(screen.getByTestId("rating-stars")).toHaveAttribute(
      "role",
      "radiogroup"
    );
    expect(axios.get).toHaveBeenCalledWith(
      "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers/b1/messages",
      expect.anything()
    );
  });

  it("puts the chat in its own post under the beer, with the rating on the beer's post", async () => {
    mockGet(openSeason, [chatMessage]);
    renderPage("b1");

    await screen.findByText("Sætur en góður.");
    const chatPost = screen.getByTestId("chat").closest("article.post");
    const beerPost = screen.getByTestId("rating-stars").closest("article.post");
    expect(chatPost).not.toBeNull();
    expect(chatPost).not.toBe(beerPost);
    expect(beerPost?.compareDocumentPosition(chatPost as Node)).toBe(
      Node.DOCUMENT_POSITION_FOLLOWING
    );
    expect(
      within(chatPost as HTMLElement).getByRole("heading", { name: "Spjall" })
    ).toBeInTheDocument();
    expect(chatPost).toHaveTextContent("1 skilaboð");
    expect(
      within(beerPost as HTMLElement).getByTestId("beer-details")
    ).toBeInTheDocument();
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
