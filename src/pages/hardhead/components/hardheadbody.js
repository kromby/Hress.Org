import React, { useEffect, useState } from "react"
import config from 'react-global-configuration';
import Guests from "./guests";
import Movie from "./movie";

const HardheadBody = (propsData) => {
    const [moviePhoto, setMoviePhoto] = useState();
    const [viewNight, setViewNight] = useState(true);
    const [viewMovie, setViewMovie] = useState(true);
    const [viewGuests, setViewGuests] = useState(true);

    useEffect(() => {
        if (propsData.viewNight !== undefined) {
            setViewNight(propsData.viewNight);
        }

        if(propsData.viewMovie !== undefined) {
            setViewMovie(propsData.viewMovie);
        }

        if(propsData.viewGuests !== undefined) {
            setViewGuests(propsData.viewGuests);
        }
    }, [propsData])

    const photoPostback = async (src) => {
        if (src) {
            setMoviePhoto(src);
        }
    }

    return (
        [
            moviePhoto ?
                <span key="A" className="image right"><img src={config.get('apiPath') + moviePhoto} alt={propsData.Name} style={{height: propsData.imageHeight}} /></span> : null,
            viewNight ?
                <section key="0">
                    <h3>Kvöldið</h3>
                    <p>
                        {propsData.description ? propsData.description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
                    </p>
                </section> : null,
            viewMovie ? <Movie key="1" id={propsData.id} photoPostback={photoPostback} /> : null,
            viewGuests ? <Guests key="2" hardheadID={propsData.id} /> : null,
            <p key="3"></p>,
        ]
    )
}

export default HardheadBody;