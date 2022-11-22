import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Album from "./album";
import { isMobile } from "react-device-detect";

const AlbumImages = (propsData) => {
    const [images, setImages] = useState();
    const [selectedImage, setSelectedImage] = useState();

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

    const handleChange = async (id) => {
        setSelectedImage(id);
        window.scrollTo({top: isMobile ? 250 : 350, left: 0, behavior: 'smooth'});
    }    

    return (
        <section>
            <div className="box alt">
                <div className="row gtr-uniform">
                    <div class="col-12">
                        <span class="image album">
                            <img src={config.get("apiPath") + "/api/images/" + selectedImage + "/content"} alt="" />
                        </span>
                    </div>
                    {images ? images.map(image =>
                        <div className={isMobile ? "col-3" : "col-2"}>
                            <span className="image fit" key={image.id}>
                                <img src={config.get("apiPath") + "/api/images/" + image.id + "/content"} alt={image.name} onClick={() => handleChange(image.id)} />
                            </span>
                        </div>
                    ) : null}
                </div>
            </div>
        </section>
    )
}

export default AlbumImages;