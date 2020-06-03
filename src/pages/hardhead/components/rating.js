import React, {useEffect, useState} from 'react';
import config from 'react-global-configuration';
import { useAuth } from '../../../context/auth';
import axios from "axios";

const Rating = (propsData) => {
    const {authTokens} = useAuth();
    const [data, setData] = useState({ratings: [], isLoading: false});

    useEffect(() => {
        const getRatingData = async  () => {
            if(authTokens !== undefined){ 
                console.log("Rating authTokens: " + authTokens);
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

    return (
        <div>
            {authTokens && data.visible ? 
            <span>
                {data.ratings.HardheadRating}
                <br/>
                {data.ratings.MovieRating}
            </span>  : "Stjörnugjöf væntanleg á næstunni"}
        </div>
    )
}

export default Rating;