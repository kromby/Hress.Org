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
import RegisterBeerPage from "./register";
import { AuthContext } from "../../context/auth";
import BeerPage from "./beer";
import { DuplicateBeerError, seasonBeersKey } from "./hooks/useBeerApi";
import { Beer } from "./types";

const mockUploadPhoto = jest.fn();
const mockAddToAlbum = jest.fn();
jest.mock("./hress", () => ({
  ...jest.requireActual("./hress"),
  useHressPhotos: () => ({
    uploadPhoto: (...args: unknown[]) => mockUploadPhoto(...args),
    addToAlbum: (...args: unknown[]) => mockAddToAlbum(...args),
  }),
}));

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

  let realBeerPage = false;

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
                element={
                  realBeerPage ? (
                    <BeerPage />
                  ) : (
                    <p data-testid="beer-page">beer page</p>
                  )
                }
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
    realBeerPage = false;
    mockUploadPhoto.mockResolvedValue(279080);
    mockAddToAlbum.mockImplementation(() => Promise.resolve());
    URL.createObjectURL = jest.fn(() => "blob:preview");
    URL.revokeObjectURL = jest.fn();
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
    (axios.post as jest.Mock).mockImplementation(
      () => new Promise(() => undefined)
    );
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

  it("shows the add error right under the matches, before the form", async () => {
    (axios.post as jest.Mock).mockRejectedValue({ response: { status: 500 } });
    renderPage();
    await typeName();

    fireEvent.click(screen.getByRole("button", { name: /Bæta við/ }));

    const alert = await screen.findByRole("alert");
    const matchesList = screen.getAllByTestId("beer-matches")[0];
    const form = screen
      .getByRole("button", { name: "Skrá bjór" })
      .closest("form");
    expect(
      matchesList.compareDocumentPosition(alert) &
        Node.DOCUMENT_POSITION_FOLLOWING
    ).toBeTruthy();
    expect(
      alert.compareDocumentPosition(form as HTMLElement) &
        Node.DOCUMENT_POSITION_FOLLOWING
    ).toBeTruthy();
  });

  it("says the season is closed when adding gets a closed-season 409", async () => {
    (axios.post as jest.Mock).mockRejectedValue({
      response: { status: 409, data: "Season is closed" },
    });
    renderPage();
    await typeName();

    fireEvent.click(screen.getByRole("button", { name: /Bæta við/ }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Tímabilið er búið og ekki hægt að skrá bjór."
    );
  });

  describe("new beer form", () => {
    const newBeer = {
      id: "n1",
      name: "Jóla Gull",
      producer: "Ölgerðin",
      country: "Ísland",
      alcoholPercentage: 5.4,
    };

    const fill = async () => {
      fireEvent.change(await screen.findByLabelText("Nafn"), {
        target: { value: "Jóla Gull" },
      });
      fireEvent.change(screen.getByLabelText("Brugghús"), {
        target: { value: "Ölgerðin" },
      });
      fireEvent.change(screen.getByLabelText("Land"), {
        target: { value: "Ísland" },
      });
      fireEvent.change(screen.getByLabelText("Styrkleiki %"), {
        target: { value: "5,4" },
      });
    };

    const pickPhoto = (file: File) =>
      fireEvent.change(screen.getByLabelText("Taka mynd af dósinni"), {
        target: { files: [file] },
      });

    const photo = () => new File(["x"], "can.jpg", { type: "image/jpeg" });
    const save = () =>
      fireEvent.click(screen.getByRole("button", { name: "Skrá bjór" }));

    it("saves a new beer without a photo and opens its page", async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: newBeer });
      renderPage();
      await fill();

      save();

      expect(await screen.findByTestId("beer-page")).toBeInTheDocument();
      expect(axios.post).toHaveBeenCalledWith(
        expect.stringContaining("/seasons/jolabjor-2026/beers"),
        {
          beer: {
            name: "Jóla Gull",
            producer: "Ölgerðin",
            country: "Ísland",
            alcoholPercentage: 5.4,
          },
        },
        expect.anything()
      );
      expect(mockUploadPhoto).not.toHaveBeenCalled();
    });

    it("shows validation errors and doesn't post", async () => {
      renderPage();
      await screen.findByLabelText("Nafn");

      save();

      expect(await screen.findAllByText(/er nauðsynlegt/)).not.toHaveLength(0);
      expect(axios.post).not.toHaveBeenCalled();
    });

    it("uploads the photo to album 46153 and saves it as the cover", async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: newBeer });
      renderPage();
      await fill();
      const file = photo();
      pickPhoto(file);
      expect(await screen.findByAltText("Mynd af dósinni")).toBeInTheDocument();

      save();

      expect(await screen.findByTestId("beer-page")).toBeInTheDocument();
      expect(mockUploadPhoto).toHaveBeenCalledWith(file, "Jóla Gull");
      expect(mockAddToAlbum).toHaveBeenCalledWith(279080);
      expect((axios.post as jest.Mock).mock.calls[0][1].beer.imageID).toBe(
        279080
      );
    });

    it("still saves the beer when adding the photo to the album fails", async () => {
      const consoleError = jest
        .spyOn(console, "error")
        .mockImplementation(() => undefined);
      mockAddToAlbum.mockRejectedValue(new Error("nope"));
      (axios.post as jest.Mock).mockResolvedValue({ data: newBeer });
      renderPage();
      await fill();
      pickPhoto(photo());

      save();

      expect(await screen.findByTestId("beer-page")).toBeInTheDocument();
      expect((axios.post as jest.Mock).mock.calls[0][1].beer.imageID).toBe(
        279080
      );
      expect(consoleError).toHaveBeenCalled();
      consoleError.mockRestore();
    });

    it("keeps the input and doesn't re-upload when saving fails", async () => {
      (axios.post as jest.Mock)
        .mockRejectedValueOnce({ response: { status: 500 } })
        .mockResolvedValueOnce({ data: newBeer });
      renderPage();
      await fill();
      pickPhoto(photo());

      save();

      expect(await screen.findByRole("alert")).toHaveTextContent(
        "Ekki tókst að skrá bjórinn. Reyndu aftur."
      );
      expect(screen.getByLabelText("Nafn")).toHaveValue("Jóla Gull");
      expect(screen.getByLabelText("Brugghús")).toHaveValue("Ölgerðin");
      expect(screen.getByLabelText("Styrkleiki %")).toHaveValue("5,4");

      save();

      expect(await screen.findByTestId("beer-page")).toBeInTheDocument();
      expect(mockUploadPhoto).toHaveBeenCalledTimes(1);
      expect(mockAddToAlbum).toHaveBeenCalledTimes(1);
      expect(axios.post).toHaveBeenCalledTimes(2);
      expect((axios.post as jest.Mock).mock.calls[1][1].beer.imageID).toBe(
        279080
      );
    });

    it("disables Skrá bjór while saving", async () => {
      (axios.post as jest.Mock).mockImplementation(
        () => new Promise(() => undefined)
      );
      renderPage();
      await fill();

      save();

      await waitFor(() =>
        expect(screen.getByRole("button", { name: "Skrá bjór" })).toBeDisabled()
      );
      save();
      expect(axios.post).toHaveBeenCalledTimes(1);
    });

    it("shows the existing beer when it's a duplicate", async () => {
      (axios.post as jest.Mock).mockRejectedValue({
        response: { status: 409, data: { duplicate: newBeer } },
      });
      renderPage();
      await fill();

      save();

      const heading = await screen.findByText("Þessi bjór er þegar til:");
      expect(heading).toBeInTheDocument();
      const list = screen
        .getAllByTestId("beer-matches")
        .find((l) => l.textContent?.includes("Ölgerðin · 5,4%"));
      expect(list).toBeDefined();
      expect(
        within(list as HTMLElement).getByRole("button", { name: /Bæta við/ })
      ).toBeInTheDocument();
      expect(screen.queryByTestId("beer-page")).not.toBeInTheDocument();
    });

    it("scrolls the duplicate panel into view and clears it when saving again", async () => {
      const scrollIntoView = jest.fn();
      Element.prototype.scrollIntoView = scrollIntoView;
      (axios.post as jest.Mock)
        .mockRejectedValueOnce({
          response: { status: 409, data: { duplicate: newBeer } },
        })
        .mockImplementationOnce(() => new Promise(() => undefined));
      renderPage();
      await fill();

      save();
      await screen.findByText("Þessi bjór er þegar til:");
      expect(scrollIntoView).toHaveBeenCalledTimes(1);
      expect(scrollIntoView.mock.instances[0]).toBe(
        screen.getByText("Þessi bjór er þegar til:").parentElement
      );

      save();
      await waitFor(() =>
        expect(
          screen.queryByText("Þessi bjór er þegar til:")
        ).not.toBeInTheDocument()
      );
      delete (Element.prototype as Partial<Element>).scrollIntoView;
    });

    it("focuses Nafn when the name is the first error", async () => {
      renderPage();
      await screen.findByLabelText("Nafn");

      save();

      await waitFor(() => expect(screen.getByLabelText("Nafn")).toHaveFocus());
    });

    it("focuses the first invalid field below Nafn", async () => {
      renderPage();
      fireEvent.change(await screen.findByLabelText("Nafn"), {
        target: { value: "Jóla Gull" },
      });
      fireEvent.change(screen.getByLabelText("Brugghús"), {
        target: { value: "Ölgerðin" },
      });

      save();

      await waitFor(() => expect(screen.getByLabelText("Land")).toHaveFocus());
    });

    it("lays out Brugghús full width and Land beside Styrkleiki %", async () => {
      renderPage();
      await screen.findByLabelText("Nafn");

      const col = (label: string) =>
        screen.getByLabelText(label).parentElement as HTMLElement;
      expect(col("Brugghús")).toHaveClass("col-12");
      expect(col("Land")).toHaveClass("col-6", "col-12-small");
      expect(col("Styrkleiki %")).toHaveClass("col-6", "col-12-small");
    });

    it("keeps every field in one grid, with themed inputs", async () => {
      renderPage();
      await screen.findByLabelText("Nafn");

      // Every new-beer field sits in the same grid, so the gaps match.
      const grid = screen.getByLabelText("Brugghús").closest(".row");
      for (const label of [
        "Taka mynd af dósinni",
        "Vínbúðin (valfrjálst)",
        "Tegund (valfrjálst)",
        "Lýsing (valfrjálst)",
      ]) {
        expect(screen.getByLabelText(label).closest(".row")).toBe(grid);
      }
      // The theme only styles text inputs; inputMode keeps the URL keyboard.
      expect(screen.getByLabelText("Vínbúðin (valfrjálst)")).toHaveAttribute(
        "type",
        "text"
      );
      expect(screen.getByLabelText("Vínbúðin (valfrjálst)")).toHaveAttribute(
        "inputMode",
        "url"
      );
      // The photo picker is a theme button, not the browser's "Browse…".
      expect(screen.getByText("Taka mynd af dósinni")).toHaveClass("button");
    });

    it("drops the photo button's focus outline once a photo replaces the input", async () => {
      renderPage();
      await screen.findByLabelText("Nafn");
      const button = screen.getByText("Taka mynd af dósinni");

      fireEvent.focus(screen.getByLabelText("Taka mynd af dósinni"));
      expect(button.style.outline).not.toBe("");

      fireEvent.change(screen.getByLabelText("Taka mynd af dósinni"), {
        target: {
          files: [new File(["x"], "dos.jpg", { type: "image/jpeg" })],
        },
      });

      await screen.findByAltText("Mynd af dósinni");
      expect(screen.getByText("Taka mynd af dósinni").style.outline).toBe("");
    });

    it("can add the duplicate beer to the season", async () => {
      (axios.post as jest.Mock)
        .mockRejectedValueOnce(new DuplicateBeerError(newBeer as Beer))
        .mockResolvedValueOnce({ data: newBeer });
      renderPage();
      await fill();
      save();
      await screen.findByText("Þessi bjór er þegar til:");

      fireEvent.click(
        screen.getByRole("button", { name: /Bæta við Jóla Gull/ })
      );

      expect(await screen.findByTestId("beer-page")).toBeInTheDocument();
      expect(axios.post).toHaveBeenLastCalledWith(
        expect.anything(),
        { beerID: "n1" },
        expect.anything()
      );
    });

    it("shows the closed message and keeps input when the season closed meanwhile", async () => {
      (axios.post as jest.Mock).mockRejectedValue({
        response: { status: 409, data: "Season is closed" },
      });
      renderPage();
      await fill();

      save();

      expect(await screen.findByRole("alert")).toHaveTextContent(
        "Tímabilið er búið og ekki hægt að skrá bjór."
      );
      expect(screen.getByLabelText("Nafn")).toHaveValue("Jóla Gull");
    });

    it("rejects a photo over 10 MB", async () => {
      renderPage();
      await fill();
      const big = photo();
      Object.defineProperty(big, "size", { value: 11 * 1024 * 1024 });

      pickPhoto(big);

      expect(
        await screen.findByText("Mynd er of stór. Hámarksstærð er 10MB")
      ).toBeInTheDocument();
      expect(screen.queryByAltText("Mynd af dósinni")).not.toBeInTheDocument();
    });

    it("rejects a file that isn't an image", async () => {
      renderPage();
      await fill();

      pickPhoto(new File(["x"], "a.txt", { type: "text/plain" }));

      expect(
        await screen.findByText("Aðeins er hægt að hlaða upp myndum")
      ).toBeInTheDocument();
    });

    it("removes the photo", async () => {
      renderPage();
      await fill();
      pickPhoto(photo());
      await screen.findByAltText("Mynd af dósinni");

      fireEvent.click(screen.getByRole("button", { name: "Fjarlægja mynd" }));

      expect(screen.queryByAltText("Mynd af dósinni")).not.toBeInTheDocument();
    });
  });

  describe("landing on the new beer's page", () => {
    const created = {
      id: "n1",
      name: "Jóla Gull",
      producer: "Ölgerðin",
      country: "Ísland",
      alcoholPercentage: 5.4,
    };

    // The season list is cached without the new beer and its refetch is held
    // until the test resolves it.
    const holdListRefetch = () => {
      let resolveList: (beers: object[]) => void = () => undefined;
      let listCalls = 0;
      (axios.get as jest.Mock).mockImplementation((url: string) => {
        if (url.includes("/beer/beers?")) {
          return Promise.resolve({ data: matches });
        }
        if (url.endsWith("/seasons/jolabjor-2026/beers")) {
          listCalls++;
          return new Promise((resolve) => {
            resolveList = (beers) => resolve({ data: beers });
          });
        }
        if (url.endsWith("/messages")) return Promise.resolve({ data: [] });
        return Promise.resolve({ data: openSeason });
      });
      queryClient.setQueryData(seasonBeersKey(openSeason.id), []);
      return {
        listCalls: () => listCalls,
        resolve: (beers: object[]) => resolveList(beers),
      };
    };

    const flush = () => new Promise((resolve) => setTimeout(resolve, 50));

    beforeEach(() => {
      realBeerPage = true;
    });

    it("never says the beer isn't in the season after creating it", async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: created });
      const list = holdListRefetch();
      renderPage();
      fireEvent.change(await screen.findByLabelText("Nafn"), {
        target: { value: "Jóla Gull" },
      });
      fireEvent.change(screen.getByLabelText("Brugghús"), {
        target: { value: "Ölgerðin" },
      });
      fireEvent.change(screen.getByLabelText("Land"), {
        target: { value: "Ísland" },
      });
      fireEvent.change(screen.getByLabelText("Styrkleiki %"), {
        target: { value: "5,4" },
      });

      fireEvent.click(screen.getByRole("button", { name: "Skrá bjór" }));
      await waitFor(() => expect(axios.post).toHaveBeenCalled());
      await flush();

      expect(list.listCalls()).toBeGreaterThan(0);
      expect(screen.queryByTestId("beer-not-found")).not.toBeInTheDocument();

      list.resolve([{ beer: created }]);

      expect(
        await screen.findByRole("heading", { name: "Jóla Gull" })
      ).toBeInTheDocument();
      expect(screen.queryByTestId("beer-not-found")).not.toBeInTheDocument();
    });

    it("never says the beer isn't in the season after adding an existing one", async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: matches[0].beer });
      const list = holdListRefetch();
      renderPage();
      await typeName();

      fireEvent.click(screen.getByRole("button", { name: /Bæta við/ }));
      await waitFor(() => expect(axios.post).toHaveBeenCalled());
      await flush();

      expect(list.listCalls()).toBeGreaterThan(0);
      expect(screen.queryByTestId("beer-not-found")).not.toBeInTheDocument();

      list.resolve([{ beer: matches[0].beer }]);

      expect(
        await screen.findByRole("heading", { name: "Jóla Kaldi" })
      ).toBeInTheDocument();
      expect(screen.queryByTestId("beer-not-found")).not.toBeInTheDocument();
    });
  });
});
