import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../components";
import AlbumImages from "./albumImages";
import { useAuth } from "../../context/auth";
import { Redirect, useLocation } from "react-router-dom";

const Album = (propsData) => {
    const { authTokens } = useAuth();
    const { currentpath } = useLocation();
    const [album, setAlbum] = useState();

    useEffect(() => {
        const getAlbum = async () => {
            var url = config.get("apiPath") + "/api/albums/" + propsData.match.params.id;
            try {
                const response = await axios.get(url, {
					headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
				});
                setAlbum(response.data);
                document.title = "Myndir - " + response.data.name + " | Hress.Org";
            } catch (e) {
                console.error(e);
            }
        }

        if (!album) {
            getAlbum();
        }
    }, [propsData]);

    if (authTokens === undefined) {
        // return <Redirect to='/login' />
        console.log("User not logged in!");
        console.log("currentpath: ", currentpath);
        // this.context.router.push({pathname: "/login", state: { from: currentpath }});
    } else {
        return (
            <div id="main">
                {album ?
                    <Post key={album.id}
                        id={album.id}
                        title={album.name}
                        description={album.description}
                        dateFormatted={album.insertedString}
                        body={
                            <div>
                                <AlbumImages url={album.images.href} />
                            </div>
                        }
                    /> : null
                }
            </div>
        )
    }
}

export default Album;