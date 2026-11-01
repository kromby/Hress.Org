import { ReactNode } from "react";
import { Link } from "react-router-dom";
import { useHressAuth } from "./hress";

interface RequireLoginProps {
  children: ReactNode;
}

const RequireLogin = ({ children }: RequireLoginProps) => {
  const { isLoggedIn } = useHressAuth();

  if (!isLoggedIn) {
    return (
      <section data-testid="login-required">
        <p>Þú þarft að skrá þig inn til að nota bjórhlutann.</p>
        <Link to="/login">Skrá inn</Link>
      </section>
    );
  }

  return <>{children}</>;
};

export default RequireLogin;
