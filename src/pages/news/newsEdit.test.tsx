import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import NewsEdit from "./newsEdit";
import { AuthContext } from "../../context/auth";
import { useCreateNews } from "../../hooks/useCreateNews";

jest.mock("../../hooks/useCreateNews");

// The real Tiptap editor needs layout APIs jsdom lacks; a textarea stands in for it.
jest.mock("../../components/editor/richTextEditor", () => {
  const React = jest.requireActual("react");
  return {
    __esModule: true,
    default: (props: { value: string; onChange: (html: string) => void }) =>
      React.createElement("textarea", {
        "aria-label": "Meginmál",
        value: props.value,
        onChange: (e: { target: { value: string } }) =>
          props.onChange(e.target.value),
      }),
  };
});

const mockedUseCreateNews = useCreateNews as jest.Mock;

const renderPage = (loggedIn = true) =>
  render(
    <MemoryRouter initialEntries={["/news/new"]}>
      <AuthContext.Provider
        value={{ authTokens: loggedIn ? { token: "fake-token" } : undefined }}
      >
        <Routes>
          <Route path="/news/new" element={<NewsEdit />} />
          <Route path="/news/:id" element={<p>Article page</p>} />
          <Route path="/login" element={<p>Login page</p>} />
        </Routes>
      </AuthContext.Provider>
    </MemoryRouter>
  );

describe("NewsEdit", () => {
  const mutateAsync = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseCreateNews.mockReturnValue({
      mutateAsync,
      isPending: false,
      error: null,
    });
  });

  it("requires a headline and a body before saving", async () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: "Birta frétt" }));

    expect(await screen.findByText("Fyrirsögn vantar.")).toBeInTheDocument();
    expect(screen.getByText("Meginmál vantar.")).toBeInTheDocument();
    expect(mutateAsync).not.toHaveBeenCalled();
  });

  it("creates the news and opens the article", async () => {
    mutateAsync.mockResolvedValue({ id: 77 });
    renderPage();

    fireEvent.change(screen.getByLabelText("Fyrirsögn"), {
      target: { value: "  Ný frétt  " },
    });
    fireEvent.change(screen.getByLabelText("Meginmál"), {
      target: { value: "<p>Halló</p>" },
    });
    fireEvent.click(screen.getByLabelText("Opinber"));
    fireEvent.click(screen.getByRole("button", { name: "Birta frétt" }));

    await waitFor(() =>
      expect(mutateAsync).toHaveBeenCalledWith({
        name: "Ný frétt",
        content: "<p>Halló</p>",
        public: false,
      })
    );
    expect(await screen.findByText("Article page")).toBeInTheDocument();
  });

  it("shows the server error when saving fails", async () => {
    mockedUseCreateNews.mockReturnValue({
      mutateAsync,
      isPending: false,
      error: { response: { data: { error: "Headline is required" } } },
    });
    renderPage();

    expect(
      screen.getByText("Ekki tókst að vista frétt: Headline is required")
    ).toBeInTheDocument();
  });

  it("redirects to login when not authenticated", async () => {
    renderPage(false);

    expect(await screen.findByText("Login page")).toBeInTheDocument();
  });
});
