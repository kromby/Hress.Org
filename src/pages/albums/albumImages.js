import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Album from "./album";

const AlbumImages = (propsData) => {
    const [images, setImages] = useState();

    useEffect(() => {
        const getImages = async () => {
            var url = config.get("apiPath") + propsData.url;
            try {
                const response = await axios.get(url);
                setImages(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!images) {
            getImages();
        }
    }, [propsData]);

    return (
        <section>
            <div className="box alt">
                <div className="row gtr-uniform">
                    {images ? images.map(image =>
                        <div className="col-4">
                            <span className="image fit" key={image.id}>
                                <img src={config.get("apiPath") + "/api/images/" + image.id + "/content"} alt={image.name} />
                            </span>
                        </div>
                    ) : null}
                </div>
            </div>
        </section>
    )
}

export default AlbumImages;