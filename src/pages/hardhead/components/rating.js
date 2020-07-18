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

    const getRatingText = (rate) => {
        if(rate == '1')
            return 'hræðilegt kvöld';
        else if(rate == '2')
            return 'slæmt kvöld';
        else if(rate == '3')
            return 'ágætt kvöld';
        else if(rate == '4')
            return 'gott kvöld';
        else if(rate == '5')
            return 'frábært kvöld';
        else
            return '';
    }

    return (
        <ul className="stats">            
            {authTokens && data.visible ?
                <li>Einkunnir</li> :
                <li>Skráðu þig inn til þess að gefa einkunn</li>
            }
            {data.ratings !== undefined && data.ratings.Ratings ?             
                data.ratings.Ratings.map(rating => 
                    <li key={rating.Code}>                        
                        <span id={rating.Code} />
                        {rating.Code === "REP_C_RTNG" ?  
                            <i className="icon solid fa-beer fa-1x"></i> :
                            <i className="icon solid fa-film fa-1x"></i>}
                        <Rating
                            emptySymbol="far fa-star fa-1x"
                            fullSymbol="fas fa-star fa-1x"
                            initialRating={rating.MyRating ? rating.MyRating : 0}
                            readonly={data.ratings.Status === "closed"}
                            onHover={(rate) => document.getElementById(rating.Code).innerHTML = getRatingText(rate) || ' '}                            
                            onChange={(rate) => alert("Ekki tókst að vista!")}
                        />                                                
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