import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { isMobile } from "react-device-detect";
import { useAuth } from "../../context/auth";

const AlbumImages = ({url}) => {
    const { authTokens } = useAuth();
    const [images, setImages] = useState();
    const [selectedImage, setSelectedImage] = useState();

    useEffect(() => {
        const getImages = async () => {
            const _url = config.get("apiPath") + url;
            try {
                const response = await axios.get(_url, {
					headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
				});
                setImages(response.data);
            } catch (e) {
                console.error(e);
            }
        }        

        if (!images) {
            getImages();
        }
    }, [url]);

    const handleChange = (id) => {
        setSelectedImage(id);
        window.scrollTo({top: isMobile ? 250 : 350, left: 0, behavior: 'smooth'});
    }    

    return (
        <section>
            <div className="box alt">
                <div className="row gtr-uniform">
                    <div className="col-12">
                        <span className="image album">
                            <img src={config.get("apiPath") + "/api/images/" + selectedImage + "/content?width=1400"} alt="" />
                        </span>
                    </div>
                    {images ? images.map(image =>
                        <div className={isMobile ? "col-3" : "col-2"} key={image.id}>
                            <span className="image fit" key={image.id}>
                                <img src={config.get("apiPath") + "/api/images/" + image.id + "/content?width=200&height=150"} alt={image.name} onClick={() => handleChange(image.id)} />
                            </span>
                        </div>
                    ) : null}
                </div>
            </div>
        </section>
    )
}

export default AlbumImages;