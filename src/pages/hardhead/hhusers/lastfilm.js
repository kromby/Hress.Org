import React, { useState, useEffect } from 'react';
import axios from 'axios';
import config from 'react-global-configuration';
import { MiniPost } from "../../../components";

const LastFilm = (propsData) => {
    const [hardhead, setHardhead] = useState();
    const [movie, setMovie] = useState();

    useEffect(() => {
        const getLastHardhead = async () => {
            var url = config.get('path') + '/api/hardhead?userID=' + propsData.userID + '&code=' + config.get('code');

            try {
                const response = await axios.get(url);
                setHardhead(response.data[0]);
                getLastMovie(response.data[0].ID);
            } catch (e) {
                console.error(e);
            }
        }

        const getLastMovie = async (id) => {
            var url = config.get('path') + '/api/movies/' + id + '?code=' + config.get('code');

            try {
                const response = await axios.get(url);
                setMovie(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!hardhead) {
            getLastHardhead();
        }
    }, [propsData])

    return (<div>
        {hardhead ?
            <MiniPost
                title={"Nýjasta harðhausakvöld: " + hardhead.Number}
                description={<span>{hardhead.GuestCount + " gestir"}<br /><br />{hardhead.Description ? hardhead.Description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}</span>}
                date={hardhead.Date}
                dateString={hardhead.DateString}
                imageSource={movie ? config.get('apiPath') + movie.PosterPhoto.Href : null}
            /> : null}
    </div>);

}

export default LastFilm;