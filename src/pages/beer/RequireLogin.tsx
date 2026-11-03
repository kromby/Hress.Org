import { ReactNode } from "react";
import LoginLink from "./LoginLink";
import { HressPage, useHressAuth } from "./hress";

interface RequireLoginProps {
  children: ReactNode;
}

const RequireLogin = ({ children }: RequireLoginProps) => {
  const { isLoggedIn } = useHressAuth();

  if (!isLoggedIn) {
    return (
      <HressPage title="Bjór">
        <section data-testid="login-required">
          <p>Þú þarft að skrá þig inn til að nota bjórhlutann.</p>
          <LoginLink />
        </section>
      </HressPage>
    );
  }

  return children;
};

export default RequireLogin;
