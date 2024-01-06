import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { isMobile } from "react-device-detect";
import { useAuth } from "../../context/auth";

const Preview = ({ url }) => {
    const { authTokens } = useAuth();
    const [images, setImages] = useState();

    useEffect(() => {
        const getImages = async () => {
            var _url = config.get("apiPath") + url;
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

    if(!authTokens) {
        return null;
    }

    return (
        <div className="row gtr-uniform">
            {images ? images.slice(0, (isMobile ? 3 : 6)).map(image =>
                        <div className={isMobile ? "col-4" : "col-2"} key={image.id}>
                            <span className="image fit" key={image.id}>
                                <img src={config.get("apiPath") + "/api/images/" + image.id + "/content?width=150"} alt={image.name} />
                            </span>
                        </div>
                    ) : "Engar myndir"}
        </div>
    );
}

export default Preview;