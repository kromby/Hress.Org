import config from "react-global-configuration";
import { useEffect, useState, useMemo } from "react";
import { useSingleUser } from "../hooks/useSingleUser";
import { UserBasicEntity } from "../types/userBasicEntity";

interface AuthorProps {
  id: number;
  username: string;
  href?: string;
  profilePhoto?: string;
  userPath?: string;
}

const Author: React.FC<AuthorProps> = ({
  id,
  username,
  href = undefined,
  profilePhoto = undefined,
  userPath = undefined,
}) => {
  const { user: fetchedUser, loading, error } = useSingleUser(href);
  const [userURL, setUserURL] = useState(
    "http://www.hress.org/Gang/Single.aspx?Id="
  );

  const user = useMemo<UserBasicEntity | undefined>(() => {
    if (href) {
      return fetchedUser;
    }
    
    return {
      id,
      username,
      href: "",
      name: username,
      profilePhoto: profilePhoto ? {
        id: 0,
        href: profilePhoto,
      } : undefined,
      inserted: new Date().toString(),
      insertedString: "",
      insertedBy: 0,
      deleted: "",
    };
  }, [fetchedUser, href, id, username, profilePhoto]);

  useEffect(() => {
    if (userPath) setUserURL(userPath);
  }, [userPath]);

  if (href && loading) return <div>Loading...</div>;
  if (href && error) return <div>Error loading user</div>;
  if (!user) return null;

  const getImageSrc = (profilePhoto: string, mode: string) => {
    const size = mode === "Expanded" ? 50 : mode === "Compact" ? 25 : 40;
    const imagePath = !profilePhoto
      ? "/api/images/278634/content"
      : profilePhoto;
    return `${config.get("apiPath")}${imagePath}?height=${size}&width=${size}`;
  };

  return (
    <div>
      <a href={userURL + user.id} className="author">
        <span className="name">{user.username}</span>

        <img
          src={getImageSrc(user.profilePhoto?.href ?? '', "Expanded")}
          alt={username}
          className="profile-photo"
        />
      </a>
    </div>
  );
};

export default Author;
