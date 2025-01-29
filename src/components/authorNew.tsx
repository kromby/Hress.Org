import axios from "axios";
import config from "react-global-configuration";
import { useEffect, useState } from "react";
import { TransactionUser } from "../types/balanceSheet";

interface AuthorProps {
  id: number;
  username: string;
  href?: string | null;
  profilePhoto?: string | null;
  userPath?: string | null;
}

const AuthorNew: React.FC<AuthorProps> = ({
  id,
  username,
  href = null,
  profilePhoto = null,
  userPath = null,
}) => {
  const [user, setUser] = useState<TransactionUser>();
  const [userURL, setUserURL] = useState(
    "http://www.hress.org/Gang/Single.aspx?Id="
  );

  useEffect(() => {
    const getUser = async () => {
      if (href) {
        const url = config.get("apiPath") + href;
        try {
          const response = await axios.get(url);
          setUser(response.data);
        } catch (error) {
          console.error("Error fetching user:", error);
        }
      } else {
        setUser({
          id,
          username,
          href: "",
          name: username,
          profilePhoto: {
            id: 0,
            href: profilePhoto,
          },
        });
      }
    };
    getUser();

    if (userPath) setUserURL(userPath);
  }, [href]);

  const getImageSrc = (profilePhoto: string, mode: string) => {
    const size = mode === "Expanded" ? 50 : mode === "Compact" ? 25 : 40;
    const imagePath = !profilePhoto
      ? "/api/images/278634/content"
      : profilePhoto;
    return `${config.get("apiPath")}${imagePath}?height=${size}&width=${size}`;
  };

  return (
    <div>
      {user ? (
        <a href={userURL + user.id} className="author">
          <span className="name">{user.username}</span>

        <img
          src={getImageSrc(user.profilePhoto?.href ?? '', "Expanded")}
          alt={username}
          className="profile-photo"
        />
        </a>

        ) : null}
    </div>
  );
};

export default AuthorNew;
