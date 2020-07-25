import React, {useEffect, useState} from 'react';
import config from 'react-global-configuration';
import { useAuth } from '../../../context/auth';
import axios from "axios";
import Rating from 'react-rating';

const HardheadRating = (propsData) => {
    const {authTokens} = useAuth();
    const [data, setData] = useState({ratings: [], isLoading: false});

    useEffect(() => {
        const getRatingData = async  () => {
            if(authTokens !== undefined){ 
                try {
                    var url = config.get('path') + '/api/hardhead/ratings/' + propsData.id + '?code=' + config.get('code');                    
                    const response = await axios.get(url, {
                        headers: {'Authorization': 'token ' + authTokens.token}               
                    })
                    setData({ratings: response.data, isLoading: false, visible: true})
                }
                catch(e) {
                    console.error(e);
                    setData({isLoading: false, visible: false});
                }
            }
        }
        getRatingData();
    }, [propsData, authTokens])

    const getRatingText = (rate, type) => {
        console.log("getRatingText - " + rate); 
        if(rate == '1')
            return type === 'REP_C_RTNG' ? 'hræðilegt kvöld ' : 'hræðileg mynd ';
        else if(rate == '2')
            return type === 'REP_C_RTNG' ? 'slæmt kvöld ' : 'slæm mynd ';
        else if(rate == '3')
            return type === 'REP_C_RTNG' ? 'ágætt kvöld ' : 'ágæt mynd ';
        else if(rate == '4')
            return type === 'REP_C_RTNG' ? 'gott kvöld ' : 'góð mynd ';
        else if(rate == '5')
            return type === 'REP_C_RTNG' ? 'frábært kvöld ' : 'frábær mynd ';
        else
            return '';
    }

    return (
        <ul className="stats">            
            {authTokens && data.visible ?
                null :
                <li>Skráðu þig inn til þess að gefa einkunn</li>
            }
            {data.ratings !== undefined && data.ratings.Ratings ?             
                data.ratings.Ratings.map(rating => 
                    <li key={rating.Code}>                        
                        <span id={rating.Code + "_" + propsData.id} />
                        {rating.Code === "REP_C_RTNG" ?  
                            <i className="icon solid fa-beer fa-2x"></i> :
                            <i className="icon solid fa-film fa-2x"></i>}
                        {data.ratings.Readonly ?
                        <span>({rating.NumberOfRatings} atkvæði)&nbsp;</span>
                        : null}
                        {data.ratings.Readonly && rating.MyRating === undefined && rating.AverageRating === undefined ? null :
                        <Rating
                            emptySymbol="far fa-star fa-1x"
                            fullSymbol="fas fa-star fa-1x"
                            initialRating={rating.MyRating ? rating.MyRating : 0}
                            readonly={data.ratings.Readonly}
                            onHover={(rate) => document.getElementById(rating.Code + "_" + propsData.id).innerHTML = getRatingText(rate, rating.Code) || ' '}                            
                            onChange={(rate) => alert("Ekki tókst að vista!")}
                        />}
                    </li>
                ) :
                null
            }                                        
        </ul>
    )
}

export default HardheadRating;

{/* Hardhead={data.ratings.HardheadRating}
                <Rating
                    emptySymbol="far fa-star fa-1x"
                    fullSymbol="fas fa-star fa-1x"
                    initialRating={data.ratings.Night ? data.ratings.Night.MyRating : 0}
                    {...data.ratings.Status === "closed" ? "readonly" : null}
                />  
                <br/>
                Movie={data.ratings.MovieRating}
                <Rating
                    emptySymbol="far fa-star fa-1x"
                    fullSymbol="fas fa-star fa-1x"
                    initialRating={data.ratings.Movie ? data.ratings.Movie.MyRating : 0}
                />   */}