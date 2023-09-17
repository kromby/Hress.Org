import React, { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import { useAuth } from '../../../context/auth';
import axios from "axios";
import Rating from 'react-rating';

const HardheadRating = ({id, nightRatingVisible, movieRatingVisible}) => {
    const { authTokens } = useAuth();
    const [data, setData] = useState({ ratings: [], isLoading: false });
    const [showNightRating, setShowNightRating] = useState(true);
    const [showMovieRating, setShowMovieRating] = useState(true);
    const [lastLoggedIn, setLastLoggedIn] = useState(false);

    useEffect(() => {
        if (nightRatingVisible) {
            setShowNightRating(nightRatingVisible);
        }
        if (movieRatingVisible) {
            setShowMovieRating(movieRatingVisible);
        }

        const getRatingData = async () => {
            if (authTokens !== undefined) {
                try {
                    var url = config.get('path') + '/api/hardhead/' + id + '/ratings?code=' + config.get('code');
                    const response = await axios.get(url, {
                        headers: { 'Authorization': 'token ' + authTokens.token }
                    })
                    setData({ ratings: response.data, isLoading: false, visible: true })
                }
                catch (e) {
                    console.error(e);
                    setData({ isLoading: false, visible: false });
                }
            }
        }

        var loggedIn = authTokens ? true : false;

        if (!data.ratings || lastLoggedIn != loggedIn) {
            getRatingData();
            setLastLoggedIn(loggedIn);
        }
    }, [id, nightRatingVisible, movieRatingVisible, authTokens])

    const getRatingText = (rate, type) => {
        if (rate === 1)
            return type === 'REP_C_RTNG' ? 'hræðilegt kvöld ' : 'hræðileg mynd ';
        else if (rate === 2)
            return type === 'REP_C_RTNG' ? 'slæmt kvöld ' : 'slæm mynd ';
        else if (rate === 3)
            return type === 'REP_C_RTNG' ? 'ágætt kvöld ' : 'ágæt mynd ';
        else if (rate === 4)
            return type === 'REP_C_RTNG' ? 'gott kvöld ' : 'góð mynd ';
        else if (rate === 5)
            return type === 'REP_C_RTNG' ? 'frábært kvöld ' : 'frábær mynd ';
        else
            return '';
    }

    const saveRating = async (rate, type) => {
        if (authTokens !== undefined) {
            try {
                var url = config.get('path') + '/api/hardhead/' + id + '/ratings?code=' + config.get('code');
                const response = await axios.post(url, {
                    type: type,
                    rating: rate
                }, {
                    headers: { 'Authorization': 'token ' + authTokens.token },
                });
                console.log("saveRating" + response);
            }
            catch (e) {
                console.error(e);
                alert("Ekki tókst að vista einkunn, reyndu aftur síðar.");
            }
        }
    }

    return (
        <ul className="stats">
            {authTokens && data.visible ?
                null :
                <li>Skráðu þig inn til þess að gefa einkunn</li>
            }
            {data.ratings !== undefined && data.ratings.Ratings ?
                data.ratings.Ratings.map(rating =>
                    ((rating.Code === "REP_C_RTNG" && showNightRating === true) || (rating.Code === "REP_C_MRTNG" && showMovieRating === true)) ?
                        <li key={rating.Code}>
                            <span id={rating.Code + "_" + id} />
                            {rating.Code === "REP_C_RTNG" ?
                                <i className="icon solid fa-beer fa-2x"></i> :
                                <i className="icon solid fa-film fa-2x"></i>}
                            {data.ratings.Readonly ?
                                <span>(Fjöldi: {rating.NumberOfRatings}{rating.MyRating ? ' -  Þú: ' + rating.MyRating : null})&nbsp;</span>
                                : null}
                            {data.ratings.Readonly && rating.MyRating === undefined && rating.AverageRating === undefined ? null :
                                <Rating
                                    emptySymbol="far fa-star fa-1x"
                                    fullSymbol="fas fa-star fa-1x"
                                    initialRating={data.ratings.Readonly ? rating.AverageRating : rating.MyRating}
                                    readonly={data.ratings.Readonly}
                                    onHover={(rate) => document.getElementById(rating.Code + "_" + id).innerHTML = getRatingText(rate, rating.Code) || ' '}
                                    onChange={(rate) => saveRating(rate, rating.Code)}
                                />}
                        </li>
                        : null
                ) : null
            }
        </ul>
    )
}

export default HardheadRating;