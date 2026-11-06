import { render, screen, waitFor, within } from "@testing-library/react";
import axios from "axios";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import SeasonBeerList from "./SeasonBeerList";
import { AuthContext } from "../../../context/auth";
import { Season } from "../types";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const openSeason: Season = {
  id: "jolabjor-2026",
  name: "Jólabjór 2026",
  startDate: "2026-11-01T00:00:00Z",
  isOpen: true,
};

// The backend omits null values, so an unrated beer has no myRating.
const summaries = [
  {
    beer: {
      id: "b1",
      name: "Egils Jólabjór",
      producer: "Ölgerðin",
      alcoholPercentage: 5.6,
      country: "Ísland",
    },
    myRating: 4,
  },
  {
    beer: {
      id: "b2",
      name: "Tuborg Lille Jul",
      producer: "Carlsberg",
      alcoholPercentage: 5.4,
      country: "Danmörk",
    },
  },
];

// Every post on the page except the season's own.
const findBeerPosts = async () =>
  (await screen.findAllByRole("article")).filter(
    (post) =>
      within(post).queryByRole("heading", { name: "Jólabjór 2026" }) === null
  );

describe("SeasonBeerList", () => {
  let queryClient: QueryClient;

  const renderComponent = (season: Season = openSeason, url = "/beer") =>
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={[url]}>
          <AuthContext.Provider value={{ authTokens: { token: "fake-token" } }}>
            <SeasonBeerList season={season} />
          </AuthContext.Provider>
        </MemoryRouter>
      </QueryClientProvider>
    );

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    (axios.get as jest.Mock).mockResolvedValue({ data: summaries });
  });

  it("fetches the season's beers with the Hress token header", async () => {
    renderComponent();

    await waitFor(() => {
      expect(axios.get).toHaveBeenCalledWith(
        "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers",
        { headers: { "X-Custom-Authorization": "token fake-token" } }
      );
    });
  });

  it("shows each beer as a hress.org post with its name, producer and alcohol %", async () => {
    renderComponent();

    await screen.findAllByRole("link", { name: "Egils Jólabjór" });
    const posts = await findBeerPosts();
    expect(posts).toHaveLength(2);
    expect(posts[0]).toHaveClass("post");
    expect(within(posts[0]).getByRole("heading")).toHaveTextContent(
      "Egils Jólabjór"
    );
    expect(posts[0]).toHaveTextContent("Ölgerðin · 5,6%");
    expect(posts[0]).not.toHaveTextContent("Ísland");
    expect(
      within(posts[0]).getByRole("link", { name: "Egils Jólabjór" })
    ).toHaveAttribute("href", "/beer/b1");
  });

  it("has no search box (the nav bar search is used instead)", async () => {
    renderComponent();
    await screen.findAllByRole("link", { name: "Egils Jólabjór" });

    expect(screen.queryByRole("searchbox")).not.toBeInTheDocument();
  });

  it("shows 0% for a beer whose alcohol % the backend omitted", async () => {
    (axios.get as jest.Mock).mockResolvedValue({
      data: [
        {
          beer: {
            id: "b3",
            name: "Malt og Appelsín",
            producer: "Ölgerðin",
            country: "Ísland",
          },
        },
      ],
    });

    renderComponent();

    await screen.findByRole("link", { name: "Malt og Appelsín" });
    const [post] = await findBeerPosts();
    expect(post).toHaveTextContent("Ölgerðin · 0%");
  });

  it("shows my stars on beers I have rated, and marks the rest unrated", async () => {
    renderComponent();

    await screen.findAllByRole("link", { name: "Egils Jólabjór" });
    const rows = await findBeerPosts();
    expect(within(rows[0]).getByTestId("my-rating")).toHaveAttribute(
      "aria-label",
      expect.stringContaining("4")
    );
    expect(within(rows[0]).queryByTestId("not-rated")).not.toBeInTheDocument();
    expect(within(rows[1]).getByTestId("not-rated")).toBeInTheDocument();
    expect(within(rows[1]).queryByTestId("my-rating")).not.toBeInTheDocument();
  });

  it("offers to rate a beer I haven't tasted, linking to its page", async () => {
    renderComponent();

    await screen.findAllByRole("link", { name: "Egils Jólabjór" });
    const rows = await findBeerPosts();
    expect(within(rows[1]).getByTestId("not-rated")).toHaveTextContent(
      "Ósmakaður"
    );
    expect(
      within(rows[1]).getByRole("link", {
        name: "Gefa Tuborg Lille Jul einkunn",
      })
    ).toHaveAttribute("href", "/beer/b2");
    expect(within(rows[0]).queryByTestId("rate-beer")).not.toBeInTheDocument();
  });

  it("filters by the nav bar search query, ignoring case and spaces", async () => {
    renderComponent(openSeason, "/beer?query=%20%20LILLE%20");

    await screen.findByRole("link", { name: "Tuborg Lille Jul" });
    const posts = await findBeerPosts();
    expect(posts).toHaveLength(1);
    expect(posts[0]).toHaveTextContent("Tuborg Lille Jul");
    expect(screen.getByTestId("search-status")).toHaveTextContent(
      "Bjórar sem passa við „LILLE“."
    );
    expect(screen.getByRole("link", { name: "Sýna alla" })).toHaveAttribute(
      "href",
      "/beer"
    );
  });

  it("matches the search query against the producer too", async () => {
    renderComponent(openSeason, "/beer?query=%C3%B6lger%C3%B0in");

    await screen.findByRole("link", { name: "Egils Jólabjór" });
    const posts = await findBeerPosts();
    expect(posts).toHaveLength(1);
    expect(posts[0]).toHaveTextContent("Egils Jólabjór");
  });

  it("says so when no beer matches the search query", async () => {
    renderComponent(openSeason, "/beer?query=pilsner");

    expect(await screen.findByTestId("search-status")).toHaveTextContent(
      "Enginn bjór fannst fyrir „pilsner“."
    );
    expect(await findBeerPosts()).toHaveLength(0);
  });

  it("shows the register button below the list, linking to the register page", async () => {
    renderComponent();
    await screen.findAllByRole("link", { name: "Egils Jólabjór" });

    const button = screen.getByTestId("register-beer");
    expect(button).toHaveAttribute("href", "/beer/register");
    const rows = await findBeerPosts();
    const lastRow = rows[rows.length - 1];
    expect(
      lastRow.compareDocumentPosition(button) & Node.DOCUMENT_POSITION_FOLLOWING
    ).toBeTruthy();
  });

  it("keeps the register button when the season has no beers yet", async () => {
    (axios.get as jest.Mock).mockResolvedValue({ data: [] });

    renderComponent();

    expect(await screen.findByTestId("no-beers")).toBeInTheDocument();
    expect(screen.getByTestId("register-beer")).toBeInTheDocument();
  });

  it("hides the register button when the season is closed", async () => {
    renderComponent({ ...openSeason, isOpen: false });
    await screen.findAllByRole("link", { name: "Egils Jólabjór" });

    expect(screen.getByTestId("season-closed")).toBeInTheDocument();
    expect(screen.queryByTestId("register-beer")).not.toBeInTheDocument();
  });

  it("shows an error when the beers can't be fetched", async () => {
    (axios.get as jest.Mock).mockRejectedValue({ response: { status: 500 } });

    renderComponent();

    expect(await screen.findByTestId("beers-error")).toBeInTheDocument();
  });
});
