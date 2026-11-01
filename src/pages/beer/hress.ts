// The only file in src/pages/beer that depends on the rest of hress.org
// (login, current user, API base URL). Extracting BeerRater means swapping this file.
import config from "react-global-configuration";
import { useAuth } from "../../context/auth";

export interface HressAuth {
  isLoggedIn: boolean;
  userID?: number;
  headers: Record<string, string>;
}

export const useHressAuth = (): HressAuth => {
  const { authTokens } = useAuth();
  const token: string | undefined = authTokens?.token;

  if (!token) {
    return { isLoggedIn: false, headers: {} };
  }

  const storedUserID = localStorage.getItem("userID");
  const userID = storedUserID !== null ? Number(storedUserID) : undefined;

  return {
    isLoggedIn: true,
    userID: userID !== undefined && !Number.isNaN(userID) ? userID : undefined,
    headers: { "X-Custom-Authorization": `token ${token}` },
  };
};

export const hressApiUrl = (path: string): string =>
  `${config.get("apiPath")}/api/${path}`;
