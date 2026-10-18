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
    default: (props: {
      value: string;
      onChange: (html: string) => void;
      errorId?: string;
      invalid?: boolean;
    }) =>
      React.createElement("textarea", {
        "aria-label": "Meginmál",
        "aria-describedby": props.errorId,
        "aria-invalid": props.invalid,
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

  it("links validation errors to their fields", async () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: "Birta frétt" }));

    const headline = screen.getByLabelText("Fyrirsögn");
    const body = screen.getByLabelText("Meginmál");
    const headlineError = await screen.findByText("Fyrirsögn vantar.");
    const bodyError = screen.getByText("Meginmál vantar.");

    expect(headline).toHaveAttribute("aria-invalid", "true");
    expect(headline).toHaveAttribute("aria-describedby", headlineError.id);
    expect(body).toHaveAttribute("aria-invalid", "true");
    expect(body).toHaveAttribute("aria-describedby", bodyError.id);
    expect(headlineError.id).not.toBe("");
    expect(bodyError.id).not.toBe("");
  });

  it("clears a field's error when the field is edited", async () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: "Birta frétt" }));
    expect(await screen.findByText("Fyrirsögn vantar.")).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText("Fyrirsögn"), {
      target: { value: "Ný frétt" },
    });
    fireEvent.change(screen.getByLabelText("Meginmál"), {
      target: { value: "<p>Halló</p>" },
    });

    expect(screen.queryByText("Fyrirsögn vantar.")).not.toBeInTheDocument();
    expect(screen.queryByText("Meginmál vantar.")).not.toBeInTheDocument();
    expect(screen.getByLabelText("Fyrirsögn")).toHaveAttribute(
      "aria-invalid",
      "false"
    );
    expect(screen.getByLabelText("Meginmál")).not.toHaveAttribute(
      "aria-describedby"
    );
  });

  it("only submits once when the button is clicked twice quickly", async () => {
    let resolveSave: ((value: { id: number }) => void) | undefined;
    mutateAsync.mockReturnValue(
      new Promise((resolve) => {
        resolveSave = resolve;
      })
    );
    renderPage();

    fireEvent.change(screen.getByLabelText("Fyrirsögn"), {
      target: { value: "Ný frétt" },
    });
    fireEvent.change(screen.getByLabelText("Meginmál"), {
      target: { value: "<p>Halló</p>" },
    });
    const submit = screen.getByRole("button", { name: "Birta frétt" });
    fireEvent.click(submit);
    fireEvent.click(submit);

    expect(mutateAsync).toHaveBeenCalledTimes(1);
    resolveSave?.({ id: 77 });
    expect(await screen.findByText("Article page")).toBeInTheDocument();
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

  it("shows the server error when saving fails", () => {
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
