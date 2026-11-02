import { ReactNode } from "react";
import LoginLink from "./LoginLink";
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
        <LoginLink />
      </section>
    );
  }

  return children;
};

export default RequireLogin;
