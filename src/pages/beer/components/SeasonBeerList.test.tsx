import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
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

describe("SeasonBeerList", () => {
  let queryClient: QueryClient;

  const renderComponent = (season: Season = openSeason) =>
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
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

  it("lists each beer with name, producer, country and alcohol %", async () => {
    renderComponent();

    const rows = await screen.findAllByTestId("beer-row");
    expect(rows).toHaveLength(2);
    expect(rows[0]).toHaveTextContent("Egils Jólabjór");
    expect(rows[0]).toHaveTextContent("Ölgerðin");
    expect(rows[0]).toHaveTextContent("Ísland");
    expect(rows[0]).toHaveTextContent("5,6%");
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

    expect(await screen.findByTestId("beer-row")).toHaveTextContent("0%");
  });

  it("shows my stars on beers I have rated, and marks the rest unrated", async () => {
    renderComponent();

    const rows = await screen.findAllByTestId("beer-row");
    expect(within(rows[0]).getByTestId("my-rating")).toHaveAttribute(
      "aria-label",
      expect.stringContaining("4")
    );
    expect(within(rows[0]).queryByTestId("not-rated")).not.toBeInTheDocument();
    expect(within(rows[1]).getByTestId("not-rated")).toBeInTheDocument();
    expect(within(rows[1]).queryByTestId("my-rating")).not.toBeInTheDocument();
  });

  it("filters by beer name, ignoring case and surrounding spaces", async () => {
    renderComponent();
    await screen.findAllByTestId("beer-row");

    fireEvent.change(screen.getByRole("searchbox"), {
      target: { value: "  LILLE " },
    });

    const rows = screen.getAllByTestId("beer-row");
    expect(rows).toHaveLength(1);
    expect(rows[0]).toHaveTextContent("Tuborg Lille Jul");
  });

  it("shows a no-match message when no beer name matches", async () => {
    renderComponent();
    await screen.findAllByTestId("beer-row");

    fireEvent.change(screen.getByRole("searchbox"), {
      target: { value: "pilsner" },
    });

    expect(screen.queryAllByTestId("beer-row")).toHaveLength(0);
    expect(screen.getByTestId("no-match")).toBeInTheDocument();
  });

  it("shows the register button below the list, linking to the register page", async () => {
    renderComponent();
    await screen.findAllByTestId("beer-row");

    const button = screen.getByTestId("register-beer");
    expect(button).toHaveAttribute("href", "/beer/register");
    const rows = screen.getAllByTestId("beer-row");
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
    await screen.findAllByTestId("beer-row");

    expect(screen.queryByTestId("register-beer")).not.toBeInTheDocument();
  });

  it("shows an error when the beers can't be fetched", async () => {
    (axios.get as jest.Mock).mockRejectedValue({ response: { status: 500 } });

    renderComponent();

    expect(await screen.findByTestId("beers-error")).toBeInTheDocument();
  });
});
