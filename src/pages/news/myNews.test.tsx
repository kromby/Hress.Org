import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import MyNews from "./myNews";
import { AuthContext } from "../../context/auth";
import { useMyNews } from "../../hooks/useMyNews";

jest.mock("../../hooks/useMyNews");

const mockedUseMyNews = useMyNews as jest.Mock;

const renderPage = () =>
  render(
    <MemoryRouter>
      <AuthContext.Provider value={{ authTokens: { token: "fake-token" } }}>
        <MyNews />
      </AuthContext.Provider>
    </MemoryRouter>
  );

describe("MyNews", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders a row per news item with link and public marker", () => {
    mockedUseMyNews.mockReturnValue({
      data: [
        {
          id: 42,
          name: "Public news",
          insertedString: "1. jan 2024",
          hits: 17,
          public: true,
        },
        {
          id: 43,
          name: "Private news",
          insertedString: "2. jan 2024",
          // hits and public omitted: API drops default values (0 / false)
        },
      ],
      isLoading: false,
      error: null,
    });

    renderPage();

    expect(document.title).toBe("Mínar fréttir | Hress.Org");
    const publicLink = screen.getByRole("link", { name: "Public news" });
    expect(publicLink).toHaveAttribute("href", "/news/42");
    expect(screen.getByRole("link", { name: "Private news" })).toHaveAttribute(
      "href",
      "/news/43"
    );

    const rows = screen.getAllByRole("row");
    // header + 2 data rows
    expect(rows).toHaveLength(3);
    expect(rows[1]).toHaveTextContent("17");
    expect(rows[1]).toHaveTextContent("✓");
    const privateCells = rows[2].querySelectorAll("td");
    expect(privateCells[2]).toHaveTextContent(/^0$/);
    expect(privateCells[3]).toBeEmptyDOMElement();
  });

  it("shows empty state when user has no news", () => {
    mockedUseMyNews.mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
    });

    renderPage();

    expect(
      screen.getByText("Þú hefur ekki skrifað neinar fréttir.")
    ).toBeInTheDocument();
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });
});
