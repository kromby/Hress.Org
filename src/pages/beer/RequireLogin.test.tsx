import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import RequireLogin from "./RequireLogin";
import { AuthContext } from "../../context/auth";

const renderWithAuth = (authTokens?: { token: string }) =>
  render(
    <MemoryRouter>
      <AuthContext.Provider value={{ authTokens }}>
        <RequireLogin>
          <div data-testid="protected">secret</div>
        </RequireLogin>
      </AuthContext.Provider>
    </MemoryRouter>
  );

describe("RequireLogin", () => {
  it("renders a link to /login and not its children when logged out", () => {
    renderWithAuth(undefined);

    expect(screen.getByRole("link")).toHaveAttribute("href", "/login");
    expect(screen.queryByTestId("protected")).not.toBeInTheDocument();
  });

  it("renders its children when logged in", () => {
    renderWithAuth({ token: "fake-token" });

    expect(screen.getByTestId("protected")).toBeInTheDocument();
    expect(screen.queryByRole("link")).not.toBeInTheDocument();
  });
});
