import axios from "axios";
import config from "react-global-configuration";
import { useEffect, useState } from "react";
import { UserBasicEntity } from "../types/userBasicEntity";

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
  const [user, setUser] = useState<UserBasicEntity>();
  const [userURL, setUserURL] = useState(
    "http://www.hress.org/Gang/Single.aspx?Id="
  );

  useEffect(() => {
    let isMounted = true;
    const getUser = async () => {
      if (href) {
        const url = config.get("apiPath") + href;
        try {
          const response = await axios.get(url);
          if(isMounted) {
            setUser(response.data);
          }
        } catch (error) {
          console.error("Error fetching user:", error);
        }
      } else {
        if(isMounted) {
        setUser({
          id,
          username,
          href: "",
          name: username,
          ...(profilePhoto ? {
            profilePhoto: {
              href: profilePhoto,
            }
          }:{}),
          inserted: new Date().toString(),
          insertedString: "",
          insertedBy: 0,
        });
        }
      }
    };
    getUser();

    if (userPath) setUserURL(userPath);

    return () => {
      isMounted = false;
    }
  }, [href, id, username, profilePhoto, userPath]);

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
