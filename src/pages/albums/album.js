import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../components";
import AlbumImages from "./albumImages";

const Album = (propsData) => {
    const [album, setAlbum] = useState();
    const [selectedImage, setSelectedImage] = useState();

    useEffect(() => {
        const getAlbum = async () => {
            var url = config.get("apiPath") + "/api/albums/" + propsData.match.params.id;
            try {
                const response = await axios.get(url);
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

    return (
        <div id="main">
            {album ?
                <Post key={album.id}
                    id={album.id}
                    title={album.name}
                    description={album.description}
                    dateFormatted={album.insertedString}
                    image={selectedImage ? config.get("apiPath") + "/api/images/" + selectedImage + "/content"  : null}
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

export default Album;