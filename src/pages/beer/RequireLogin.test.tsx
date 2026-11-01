import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes, useLocation } from "react-router-dom";
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
    renderWithAuth();

    expect(screen.getByRole("link")).toHaveAttribute("href", "/login");
    expect(screen.queryByTestId("protected")).not.toBeInTheDocument();
  });

  it("renders its children when logged in", () => {
    renderWithAuth({ token: "fake-token" });

    expect(screen.getByTestId("protected")).toBeInTheDocument();
    expect(screen.queryByRole("link")).not.toBeInTheDocument();
  });

  it("carries the current path as redirect-back state to /login", () => {
    const LoginProbe = () => {
      const location = useLocation();
      return <p data-testid="login-from">{location.state?.from}</p>;
    };

    render(
      <MemoryRouter initialEntries={["/beer"]}>
        <AuthContext.Provider value={{ authTokens: undefined }}>
          <Routes>
            <Route
              path="/beer"
              element={
                <RequireLogin>
                  <div data-testid="protected">secret</div>
                </RequireLogin>
              }
            />
            <Route path="/login" element={<LoginProbe />} />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    fireEvent.click(screen.getByRole("link"));

    expect(screen.getByTestId("login-from")).toHaveTextContent("/beer");
  });
});
