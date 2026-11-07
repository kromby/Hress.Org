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

// An image in the hress.org image store, scaled to the given width.
export const hressImageUrl = (imageID: number, width: number): string =>
  hressApiUrl(`images/${imageID}/content?width=${width}`);

interface HressPostProps {
  title: string;
  // Makes the title a link.
  href?: string;
  description?: ReactNode;
  // The right-hand column of the header (on phones, under the title).
  meta?: ReactNode;
  // A small image beside the title (on phones, above it).
  thumbnail?: ReactNode;
  // The post's footer, e.g. a <ul className="stats"> with a rating.
  stats?: ReactNode;
  children?: ReactNode;
}

// A post card in the hress.org layout, the same one every hress.org page uses.
export const HressPost = ({
  title,
  href,
  description,
  meta,
  thumbnail,
  stats,
  children,
}: HressPostProps) => (
  <Post
    title={title}
    href={href}
    description={description}
    meta={meta}
    thumbnail={thumbnail}
    // An empty actions list fills the footer and pushes the stats to the
    // right, as on the Hardhead pages.
    actions={stats ? <ul className="actions" /> : undefined}
    stats={stats}
    body={children}
  />
);

interface HressPageProps {
  title: string;
  description?: string;
  thumbnail?: ReactNode;
  stats?: ReactNode;
  children?: ReactNode;
  // Rendered under the post card (e.g. more posts).
  below?: ReactNode;
}

// Renders a page in the hress.org layout: the main column with a post card.
export const HressPage = ({
  title,
  description,
  thumbnail,
  stats,
  children,
  below,
}: HressPageProps) => (
  <div id="main">
    <HressPost
      title={title}
      description={description}
      thumbnail={thumbnail}
      stats={stats}
    >
      {children}
    </HressPost>
    {below}
  </div>
);
