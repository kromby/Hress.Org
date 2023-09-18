import React, { useEffect, useState } from "react"
import config from 'react-global-configuration';
import Guests from "./guests";
import Movie from "./movie";

const HardheadBody = ({id, Name, description, viewNight, viewMovie, viewGuests, imageHeight}) => {
    const [moviePhoto, setMoviePhoto] = useState();
    const [showNight, setShowNight] = useState(true);
    const [showMovie, setShowMovie] = useState(true);
    const [showGuests, setShowGuests] = useState(true);

    useEffect(() => {
        if (viewNight !== undefined) {
            setShowNight(viewNight);
        }

        if(viewMovie !== undefined) {
            setShowMovie(viewMovie);
        }

        if(viewGuests !== undefined) {
            setShowGuests(viewGuests);
        }
    }, [viewNight, viewMovie, viewGuests])

    const photoPostback = async (src) => {
        if (src) {
            setMoviePhoto(src);
        }
    }

    return (
        [
            moviePhoto ?
                <span key="A" className="image right"><img src={config.get('apiPath') + moviePhoto} alt={Name} style={{height: imageHeight}} /></span> : null,
                showNight ?
                <section key="0">
                    <h3>Kvöldið</h3>
                    <p>
                        {description ? description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
                    </p>
                </section> : null,
            showMovie ? <Movie key="1" id={id} photoPostback={photoPostback} /> : null,
            showGuests ? <Guests key="2" hardheadID={id} /> : null,
            <p key="3"></p>,
        ]
    )
}

export default HardheadBody;