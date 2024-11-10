import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../components";
import AlbumImages from "./albumImages";
import { useAuth } from "../../context/auth";
import { useLocation, useNavigate, useParams } from "react-router-dom";

const Album = () => {
  const { authTokens } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const params = useParams();
  const [album, setAlbum] = useState();

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const getAlbum = async () => {
      const url = config.get("apiPath") + "/api/albums/" + params.id;
      try {
        const response = await axios.get(url, {
          headers: { "X-Custom-Authorization": "token " + authTokens.token },
        });
        setAlbum(response.data);
        document.title = "Myndir - " + response.data.name + " | Hress.Org";
      } catch (e) {
        console.error(e);
      }
    };

    if (!album) {
      getAlbum();
    }
  }, []);

  return (
    <div id="main">
      {album ? (
        <Post
          key={album.id}
          id={album.id}
          title={album.name}
          description={album.description}
          dateFormatted={album.insertedString}
          body={
            <div>
              <AlbumImages url={album.images.href} />
              <a href={`/album/${album.id}/upload`}>
                Bæta við myndum í þetta albúm
              </a>
            </div>
          }
        />
      ) : null}
    </div>
  );
};

export default Album;
