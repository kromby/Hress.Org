import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import AwardWinners from './awardWinners';

const Awards = (propsData) => {
    const[data, setData] = useState({awards: null, isLoading: false, visible: false})

    var url = config.get('path') + '/api/hardhead/awards?code=' + config.get('code');		

    useEffect(() => {
        const getAwards = async () => {
            try {
                setData({isLoading: true});
                const response = await axios.get(url);
                setData({awards: response.data, isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }
        }

        getAwards();

    }, [propsData, url])    

    return (
        <div id="main">
            {data.visible ?
                data.awards.map((award, i) => 
                    <Post key={award.ID}
                        id={award.ID}
                        title={award.Name}
                        body={<AwardWinners href={award.Winners.Href}/>}
                    />
                ) : 
            null}
        </div>
    )
}

export default Awards;