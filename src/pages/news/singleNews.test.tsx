import { fireEvent, render, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Link, MemoryRouter, Route, Routes } from "react-router-dom";
import axios from "axios";
import SingleNews from "./singleNews";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const mockedAxios = axios as jest.Mocked<typeof axios>;

const renderPage = () =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/news/42"]}>
        <Link to="/news/43">Næsta frétt</Link>
        <Routes>
          <Route path="/news/:id" element={<SingleNews />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  );

describe("SingleNews", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("does not render script or event handler payloads from news content", async () => {
    mockedAxios.get.mockResolvedValue({
      data: {
        id: 42,
        name: "Hættuleg frétt",
        insertedString: "1. janúar 2024",
        content:
          '<p>Texti</p><script>window.hacked = true</script><img src="x" onerror="window.hacked = true">',
      },
    });

    const { container } = renderPage();

    expect(await screen.findByText("Texti")).toBeInTheDocument();
    expect(container.querySelector("script")).toBeNull();
    expect(container.querySelector("[onerror]")).toBeNull();
  });

  it("shows an edit link to the author", async () => {
    localStorage.setItem("userID", "2630");
    mockedAxios.get.mockResolvedValue({
      data: {
        id: 42,
        name: "Mín frétt",
        content: "<p>Texti</p>",
        insertedBy: 2630,
      },
    });

    renderPage();

    expect(await screen.findByRole("link", { name: "Breyta" })).toHaveAttribute(
      "href",
      "/news/42/edit"
    );
    localStorage.removeItem("userID");
  });

  it("hides the edit link from other users", async () => {
    localStorage.setItem("userID", "999");
    mockedAxios.get.mockResolvedValue({
      data: {
        id: 42,
        name: "Frétt",
        content: "<p>Texti</p>",
        insertedBy: 2630,
      },
    });

    renderPage();

    expect(await screen.findByText("Texti")).toBeInTheDocument();
    expect(
      screen.queryByRole("link", { name: "Breyta" })
    ).not.toBeInTheDocument();
    localStorage.removeItem("userID");
  });

  it("loads the new article when the id in the URL changes", async () => {
    mockedAxios.get.mockImplementation((url: string) =>
      Promise.resolve({
        data: url.endsWith("/43")
          ? { id: 43, name: "Seinni", content: "<p>Frétt 43</p>" }
          : { id: 42, name: "Fyrri", content: "<p>Frétt 42</p>" },
      })
    );

    renderPage();
    expect(await screen.findByText("Frétt 42")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("link", { name: "Næsta frétt" }));

    expect(await screen.findByText("Frétt 43")).toBeInTheDocument();
  });
});
