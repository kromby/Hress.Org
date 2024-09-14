import axios from "axios";
import config from "react-global-configuration";
import { useEffect, useState } from "react";

const Author = ({ ID, Username, href, ProfilePhoto, UserPath }) => {
  const [user, setUser] = useState();
  const [userPath, setUserPath] = useState(
    "http://www.hress.org/Gang/Single.aspx?Id="
  );

  useEffect(() => {
    const getUser = async () => {
      if (href) {
        const url = config.get("apiPath") + href;
        try {
          const response = await axios.get(url);
          setUser(response.data);
        } catch (e) {
          console.error(e);
        }
      } else {
        setUser({
          id: ID,
          username: Username,
          profilePhoto: {
            href: ProfilePhoto,
          },
        });
      }
    };

    if (UserPath) setUserPath(UserPath);

    if (!user) {
      getUser();
    }
  }, [ID]);

  const getTitle = (username, name) => {
    if (name) {
      return username + " " + name;
    } else return username;
  };

  const getImageSrc = (profilePhoto, mode) => {
    const size = mode === "Expanded" ? 50 : mode === "Compact" ? 25 : 40;
    const imagePath = !profilePhoto
      ? "/api/images/278634/content"
      : profilePhoto;
    return `${config.get("apiPath")}${imagePath}?height=${size}&width=${size}`;
  };

  return (
    <div>
      {user ? (
        <a href={userPath + user.id} className="author">
          <span className="name">{user.username}</span>

          <img
            src={getImageSrc(user.profilePhoto.href, "Expanded")}
            alt={user.username}
            title={getTitle(user.username, user.name)}
          />
        </a>
      ) : null}
    </div>
  );
};

export default Author;
