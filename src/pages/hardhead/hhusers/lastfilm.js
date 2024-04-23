import { useState, useEffect } from 'react';
import axios from 'axios';
import config from 'react-global-configuration';
import { MiniPost } from "../../../components";

const LastFilm = ({userID}) => {
    const [hardhead, setHardhead] = useState();
    const [movie, setMovie] = useState();

    useEffect(() => {
        const getLastHardhead = async () => {
            var url = config.get('apiPath') + '/api/hardhead?userID=' + userID;

            try {
                const response = await axios.get(url);
                setHardhead(response.data[0]);
                getLastMovie(response.data[0].id);
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
    }, [userID])

    return (<div>
        {hardhead ?
            <MiniPost
                title={"Nýjasta harðhausakvöld: " + hardhead.number}
                description={<span>{hardhead.guestCount + " gestir"}<br /><br />{hardhead.description ? hardhead.description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}</span>}
                date={hardhead.date}
                dateString={hardhead.dateString}
                imageSource={movie ? config.get('apiPath') + movie.PosterPhoto?.Href : null}
            /> : null}
    </div>);

}

export default LastFilm;