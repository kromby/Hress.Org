import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import axios from "axios";
import SingleNews from "./singleNews";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const mockedAxios = axios as jest.Mocked<typeof axios>;

const renderPage = () =>
  render(
    <MemoryRouter initialEntries={["/news/42"]}>
      <Routes>
        <Route path="/news/:id" element={<SingleNews />} />
      </Routes>
    </MemoryRouter>
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
});
