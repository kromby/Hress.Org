import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes, useLocation } from "react-router-dom";
import RequireLogin from "./RequireLogin";
import { AuthContext } from "../../context/auth";

const LoginProbe = () => {
  const location = useLocation();
  return <p data-testid="login-from">{location.state?.from}</p>;
};

const renderAt = (authTokens?: { token: string }) =>
  render(
    <MemoryRouter initialEntries={["/beer"]}>
      <AuthContext.Provider value={{ authTokens }}>
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

describe("RequireLogin", () => {
  it("redirects to /login with the current path as redirect-back state when logged out", () => {
    renderAt();

    expect(screen.getByTestId("login-from")).toHaveTextContent("/beer");
    expect(screen.queryByTestId("protected")).not.toBeInTheDocument();
  });

  it("renders its children when logged in", () => {
    renderAt({ token: "fake-token" });

    expect(screen.getByTestId("protected")).toBeInTheDocument();
    expect(screen.queryByTestId("login-from")).not.toBeInTheDocument();
  });
});
