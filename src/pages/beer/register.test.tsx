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
import { DuplicateBeerError } from "./hooks/useBeerApi";
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
    mockUploadPhoto.mockResolvedValue(279080);
    mockAddToAlbum.mockResolvedValue(undefined);
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
        .mockImplementation(() => {});
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
      (axios.post as jest.Mock).mockImplementation(() => new Promise(() => {}));
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
});
