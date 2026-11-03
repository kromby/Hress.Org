import { ReactNode } from "react";
import LoginRedirect from "./LoginRedirect";
import { useHressAuth } from "./hress";

interface RequireLoginProps {
  children: ReactNode;
}

const RequireLogin = ({ children }: RequireLoginProps) => {
  const { isLoggedIn } = useHressAuth();

  if (!isLoggedIn) {
    return <LoginRedirect />;
  }

  return children;
};

export default RequireLogin;
