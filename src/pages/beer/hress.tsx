// The only file in src/pages/beer that depends on the rest of hress.org
// (login, current user, API base URL, page layout). Extracting BeerRater means swapping this file.
import { ReactNode } from "react";
import config from "react-global-configuration";
import { Post } from "../../components";
import { useAuth } from "../../context/auth";

export interface HressAuth {
  isLoggedIn: boolean;
  userID?: number;
  headers: Record<string, string>;
  // Clears the stored Hress login, e.g. after the API rejects the token.
  signOut: () => void;
}

export const useHressAuth = (): HressAuth => {
  const { authTokens, setAuthTokens } = useAuth();
  const token: string | undefined = authTokens?.token;
  const signOut = () => setAuthTokens();

  if (!token) {
    return { isLoggedIn: false, headers: {}, signOut };
  }

  const storedUserID = localStorage.getItem("userID");
  const userID = storedUserID !== null ? Number(storedUserID) : undefined;

  return {
    isLoggedIn: true,
    userID: userID !== undefined && !Number.isNaN(userID) ? userID : undefined,
    headers: { "X-Custom-Authorization": `token ${token}` },
    signOut,
  };
};

export const hressApiUrl = (path: string): string =>
  `${config.get("apiPath")}/api/${path}`;

interface HressPostProps {
  title: string;
  // Makes the title a link.
  href?: string;
  description?: ReactNode;
  // The right-hand column of the header (on phones, under the title).
  meta?: ReactNode;
  children?: ReactNode;
}

// A post card in the hress.org layout, the same one every hress.org page uses.
export const HressPost = ({
  title,
  href,
  description,
  meta,
  children,
}: HressPostProps) => (
  <Post
    title={title}
    href={href}
    description={description}
    meta={meta}
    body={children}
  />
);

interface HressPageProps {
  title: string;
  description?: string;
  children?: ReactNode;
  // Rendered under the post card (e.g. more posts).
  below?: ReactNode;
}

// Renders a page in the hress.org layout: the main column with a post card.
export const HressPage = ({
  title,
  description,
  children,
  below,
}: HressPageProps) => (
  <div id="main">
    <HressPost title={title} description={description}>
      {children}
    </HressPost>
    {below}
  </div>
);
